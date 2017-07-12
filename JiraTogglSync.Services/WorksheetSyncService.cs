using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using TechTalk.JiraRestClient;

namespace JiraTogglSync.Services
{
	public class WorksheetSyncService
	{
		private readonly IExternalWorksheetRepository _source;
		private readonly IJiraRepository _target;

		public Func<IEnumerable<WorkLogEntry>, bool> AgreeToAdd = (workLogEntries) => true;
		public Func<IEnumerable<WorkLogEntry>, bool> AgreeToUpdate = (workLogEntries) => true;
		public Func<IEnumerable<WorkLogEntry>, bool> AgreeToDeleteDuplicates = (workLogEntries) => true;
		public Func<IEnumerable<WorkLogEntry>, bool> AgreeToDeleteOrphaned = (workLogEntries) => true;

		public WorksheetSyncService(IExternalWorksheetRepository source, IJiraRepository target)
		{
			_source = source;
			_target = target;

		}

		public SyncReport Syncronize(DateTime fromDate, DateTime toDate, IEnumerable<string> jiraProjectKeys, bool doPurge, int roundMinutes)
		{
			var sourceEntries = _source.GetEntries(fromDate, toDate, jiraProjectKeys);
			var targetEntries = doPurge ? _target.GetEntries(fromDate, toDate, jiraProjectKeys) : new WorkLogEntry[0];

			RoundMinutes(sourceEntries, roundMinutes);
			RoundMinutes(targetEntries, roundMinutes);

			var syncPlan = CreateSyncPlan(sourceEntries, targetEntries, doPurge);
			var syncReport = Apply(syncPlan);
			return syncReport;
		}

		private void RoundMinutes(WorkLogEntry[] entries, int roundMinutes)
		{
			foreach (var entry in entries)
			{
				entry.Round(roundMinutes);
			}
		}

		public static SyncPlan CreateSyncPlan(WorkLogEntry[] sourceEntries, WorkLogEntry[] targetEntries, bool doPurge)
		{
			if (sourceEntries == null)
				sourceEntries = new WorkLogEntry[0];

			if (targetEntries == null)
				targetEntries = new WorkLogEntry[0];

			var syncPlan = new SyncPlan();

			if (!doPurge)
			{
				syncPlan.ToAdd.AddRange(sourceEntries);
				return syncPlan;
			}

			var withoutSourceId = targetEntries.Where(e => string.IsNullOrEmpty(e.SourceId)).ToArray();
			syncPlan.ToDeleteOrphaned.AddRange(withoutSourceId);
			targetEntries = targetEntries.Except(withoutSourceId).ToArray();

			var duplicates = targetEntries
						.GroupBy(e => e.SourceId)
					.Where(g => g.Count() > 1)
					.SelectMany(g => g.ToList())
						.ToList();
			syncPlan.ToDeleteDuplicates.AddRange(duplicates);
			targetEntries = targetEntries.Except(duplicates).ToArray();

			var orphaned =
					targetEntries.Where(target => !sourceEntries.Any(source => source.SourceId == target.SourceId && source.IssueKey == target.IssueKey)).ToArray();
			syncPlan.ToDeleteOrphaned.AddRange(orphaned);
			targetEntries = targetEntries.Except(orphaned).ToArray();

			var newSourceEntries =
					sourceEntries.Where(source => targetEntries.All(target => target.SourceId != source.SourceId)).ToArray();
			syncPlan.ToAdd.AddRange(newSourceEntries);
			sourceEntries = sourceEntries.Except(newSourceEntries).ToArray();

			var sourceLookup = sourceEntries.ToDictionary(s => s.SourceId);
			var toUpdateTargetEntries = targetEntries
					.Where(target => sourceLookup.ContainsKey(target.SourceId) && target.DifferentFrom(sourceLookup[target.SourceId]))
					.Select(target => { target.Syncronize(sourceLookup[target.SourceId]); return target; }).ToList();
			syncPlan.ToUpdate.AddRange(toUpdateTargetEntries);
			targetEntries = targetEntries.Except(toUpdateTargetEntries).ToArray();
			sourceEntries = sourceEntries.Except(sourceEntries.Where(source => toUpdateTargetEntries.Any(e => e.SourceId == source.SourceId))).ToArray();

			var entriesWithNoChanges = targetEntries
					.Where(target => sourceLookup.ContainsKey(target.SourceId) && !target.DifferentFrom(sourceLookup[target.SourceId])).ToArray();
			syncPlan.NoChanges.AddRange(entriesWithNoChanges);
			targetEntries = targetEntries.Except(entriesWithNoChanges).ToArray();
			sourceEntries = sourceEntries.Except(sourceEntries.Where(source => entriesWithNoChanges.Any(e => e.SourceId == source.SourceId))).ToArray();

			//at this point only new source entries left
			syncPlan.ToAdd.AddRange(sourceEntries);

			return syncPlan;
		}


		public SyncReport Apply(SyncPlan syncPlan)
		{
			var syncReport = new SyncReport();

			if (syncPlan.ToAdd.Any() && AgreeToAdd(syncPlan.ToAdd))
				syncReport.AddedEntries = syncPlan.ToAdd.Select(item => _target.AddWorkLog(item)).ToList();

			if (syncPlan.ToUpdate.Any() && AgreeToUpdate(syncPlan.ToUpdate))
				syncReport.UpdatedEntries = syncPlan.ToUpdate.Select(item => _target.UpdateWorkLog(item)).ToList();

			if (syncPlan.ToDeleteOrphaned.Any() && AgreeToDeleteOrphaned(syncPlan.ToDeleteOrphaned))
				syncReport.DeletedOrphanedEntries = syncPlan.ToDeleteOrphaned.Select(item => _target.DeleteWorkLog(item)).ToList();

			if (syncPlan.ToDeleteDuplicates.Any() && AgreeToDeleteDuplicates(syncPlan.ToDeleteDuplicates))
				syncReport.DeletedDuplicateEntries = syncPlan.ToDeleteDuplicates.Select(item => _target.DeleteWorkLog(item)).ToList();

			syncReport.NoChanges = syncPlan.NoChanges;

			return syncReport;
		}

		public void AddWorkLog(WorkLogEntry entry)
		{
			_target.AddWorkLog(entry);
		}
	}
}