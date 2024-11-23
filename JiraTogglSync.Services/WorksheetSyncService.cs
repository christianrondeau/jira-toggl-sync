using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;

namespace JiraTogglSync.Services;

public class WorksheetSyncService
{
	public class Options
	{
		public Func<ICollection<WorkLogEntry>, bool> AgreeToAdd = _ => true;
		public Func<ICollection<WorkLogEntry>, bool> AgreeToUpdate = _ => true;
		public Func<ICollection<WorkLogEntry>, bool> AgreeToDeleteDuplicates = _ => true;
		public Func<ICollection<WorkLogEntry>, bool> AgreeToDeleteOrphaned = _ => true;
	}

	private readonly IExternalWorksheetRepository _source;
	private readonly IJiraRepository _target;
	private readonly IOptions<Options> _options;

	public WorksheetSyncService(
		IExternalWorksheetRepository source,
		IJiraRepository target,
		IOptions<Options> options
	)
	{
		_source = source;
		_target = target;
		_options = options;
	}

	public async Task<SyncReport> SynchronizeAsync(DateTimeOffset fromDate, DateTimeOffset toDate, ICollection<string> jiraProjectKeys, bool doPurge, int roundMinutes)
	{
		var sourceEntries = await _source.GetEntriesAsync(fromDate, toDate, jiraProjectKeys, roundMinutes);
		var existingWorkLogs = doPurge ? await _target.GetWorkLogOfIssuesAsync(fromDate, toDate, sourceEntries.Select(x => x.IssueKey).ToList()) : Array.Empty<WorkLogEntry>();

		var syncPlan = CreateSyncPlan(sourceEntries, existingWorkLogs, doPurge);
		var syncReport = await ApplyAsync(syncPlan);
		return syncReport;
	}

	public static SyncPlan CreateSyncPlan(WorkLogEntry[] sourceEntries, ICollection<WorkLogEntry> existingWorkLogs, bool doPurge)
	{
		var syncPlan = new SyncPlan();

		if (!doPurge)
		{
			syncPlan.ToAdd.AddRange(sourceEntries);
			return syncPlan;
		}

		var withoutSourceId = existingWorkLogs.Where(e => string.IsNullOrEmpty(e.SourceId)).ToArray();
		syncPlan.ToDeleteOrphaned.AddRange(withoutSourceId);
		existingWorkLogs = existingWorkLogs.Except(withoutSourceId).ToArray();

		var duplicates = existingWorkLogs
			.GroupBy(e => e.SourceId)
			.Where(g => g.Count() > 1)
			.SelectMany(g => g.ToList())
			.ToList();
		syncPlan.ToDeleteDuplicates.AddRange(duplicates);
		existingWorkLogs = existingWorkLogs.Except(duplicates).ToArray();

		var orphaned =
			existingWorkLogs.Where(target => !sourceEntries.Any(source => source.SourceId == target.SourceId && source.IssueKey == target.IssueKey)).ToArray();
		syncPlan.ToDeleteOrphaned.AddRange(orphaned);
		existingWorkLogs = existingWorkLogs.Except(orphaned).ToArray();

		var newSourceEntries =
			sourceEntries.Where(source => existingWorkLogs.All(target => target.SourceId != source.SourceId)).ToArray();
		syncPlan.ToAdd.AddRange(newSourceEntries);
		sourceEntries = sourceEntries.Except(newSourceEntries).ToArray();

		var sourceLookup = sourceEntries.ToDictionary(s => s.SourceId);
		var toUpdateTargetEntries = existingWorkLogs
			.Where(target => sourceLookup.ContainsKey(target.SourceId) && target.DifferentFrom(sourceLookup[target.SourceId]))
			.Select(target =>
			{
				target.Synchronize(sourceLookup[target.SourceId]);
				return target;
			}).ToList();
		syncPlan.ToUpdate.AddRange(toUpdateTargetEntries);
		existingWorkLogs = existingWorkLogs.Except(toUpdateTargetEntries).ToArray();
		sourceEntries = sourceEntries.Except(sourceEntries.Where(source => toUpdateTargetEntries.Any(e => e.SourceId == source.SourceId))).ToArray();

		var entriesWithNoChanges = existingWorkLogs
			.Where(target => sourceLookup.ContainsKey(target.SourceId) && !target.DifferentFrom(sourceLookup[target.SourceId])).ToArray();
		syncPlan.NoChanges.AddRange(entriesWithNoChanges);
		existingWorkLogs = existingWorkLogs.Except(entriesWithNoChanges).ToArray();
		sourceEntries = sourceEntries.Except(sourceEntries.Where(source => entriesWithNoChanges.Any(e => e.SourceId == source.SourceId))).ToArray();

		//at this point only new source entries left
		syncPlan.ToAdd.AddRange(sourceEntries);

		return syncPlan;
	}


	public async Task<SyncReport> ApplyAsync(SyncPlan syncPlan)
	{
		var syncReport = new SyncReport();

		if (syncPlan.ToAdd.Any() && _options.Value.AgreeToAdd(syncPlan.ToAdd))
		{
			foreach (var workLog in syncPlan.ToAdd)
				syncReport.AddedEntries.Add(await _target.AddWorkLogAsync(workLog));
		}

		if (syncPlan.ToUpdate.Any() && _options.Value.AgreeToUpdate(syncPlan.ToUpdate))
		{
			foreach (var workLog in syncPlan.ToUpdate)
				syncReport.UpdatedEntries.Add(await _target.UpdateWorkLogAsync(workLog));
		}

		if (syncPlan.ToDeleteOrphaned.Any() && _options.Value.AgreeToDeleteOrphaned(syncPlan.ToDeleteOrphaned))
		{
			foreach (var workLog in syncPlan.ToDeleteOrphaned)
				syncReport.DeletedOrphanedEntries.Add(await _target.DeleteWorkLogAsync(workLog));
		}

		if (syncPlan.ToDeleteDuplicates.Any() && _options.Value.AgreeToDeleteDuplicates(syncPlan.ToDeleteDuplicates))
		{
			foreach (var workLog in syncPlan.ToDeleteDuplicates)
				syncReport.DeletedDuplicateEntries.Add(await _target.DeleteWorkLogAsync(workLog));
		}

		syncReport.NoChanges = syncPlan.NoChanges;

		return syncReport;
	}
}