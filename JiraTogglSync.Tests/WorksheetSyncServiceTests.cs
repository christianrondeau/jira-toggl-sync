using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using JiraTogglSync.Services;
using Microsoft.Extensions.Options;
using NSubstitute;
using NUnit.Framework;

namespace JiraTogglSync.Tests;

public class WorksheetSyncServiceTests
{
	private static readonly DateTime Now = new(2022, 2, 6, 15, 15, 15, DateTimeKind.Utc);

	public static IEnumerable CreateSyncPlanTestCases()
	{
		yield return new SyncPlanScenario("Purge OFF: If no Toggl entries - make no changes",
			sourceEntries: Array.Empty<WorkLogEntry>(),
			targetEntries: Array.Empty<WorkLogEntry>(),
			doPurge: false,
			expectedResult: new SyncPlan());

		yield return new SyncPlanScenario("Purge OFF: If there are Toggl entries - add them to JIRA",
			sourceEntries: new[] { new WorkLogEntry("some-issue-key", "some-source-id", Now, 90, "New Toggl entry") },
			targetEntries: Array.Empty<WorkLogEntry>(),
			doPurge: false,
			expectedResult: new SyncPlan { ToAdd = new[] { new WorkLogEntry("some-issue-key", "some-source-id", Now, 90, "New Toggl entry") }.ToList() });

		yield return new SyncPlanScenario("Purge ON: If there are entries in JIRA, but no Toggl entries - remove JIRA entries",
			sourceEntries: Array.Empty<WorkLogEntry>(),
			targetEntries: new[] { new WorkLogEntry("some-issue-key", "some-source-id", Now, 90, "Existing JIRA entry") },
			doPurge: true,
			expectedResult:
			new SyncPlan { ToDeleteOrphaned = new[] { new WorkLogEntry("some-issue-key", "some-source-id", Now, 90, "Existing JIRA entry") }.ToList() });

		yield return new SyncPlanScenario("Purge ON: If there are no JIRA nor Toggl entries - make no changes",
			sourceEntries: Array.Empty<WorkLogEntry>(),
			targetEntries: Array.Empty<WorkLogEntry>(),
			doPurge: true,
			expectedResult: new SyncPlan());

		yield return new SyncPlanScenario("Purge ON: If there are duplicate entries in JIRA (based on toggl ID) - remove duplicates JIRA entries and add new Toggl entry",
			sourceEntries: new[] { new WorkLogEntry("some-issue-key", "123", Now, 90, "New Toggl entry") },
			targetEntries: new[]
			{
				new WorkLogEntry("some-issue-key", "123", Now, 90, "JIRA Dup 1"),
				new WorkLogEntry("some-issue-key", "123", Now, 90, "JIRA Dup 2"),
			},
			doPurge: true,
			expectedResult: new SyncPlan
			{
				ToDeleteDuplicates = new[]
				{
					new WorkLogEntry("some-issue-key", "123", Now, 90, "JIRA Dup 1"),
					new WorkLogEntry("some-issue-key", "123", Now, 90, "JIRA Dup 2"),
				}.ToList(),
				ToAdd = new[] { new WorkLogEntry("some-issue-key", "123", Now, 90, "New Toggl entry") }.ToList(),
			});

		yield return new SyncPlanScenario("Purge ON: If there are new Toggl entries - add them to JIRA",
			sourceEntries: new[] { new WorkLogEntry("some-issue-key", "123", Now, 90, "New Toggl entry") },
			targetEntries: Array.Empty<WorkLogEntry>(),
			doPurge: true,
			expectedResult: new SyncPlan { ToAdd = new[] { new WorkLogEntry("some-issue-key", "123", Now, 90, "New Toggl entry") }.ToList() });

		yield return new SyncPlanScenario("Purge ON: If Toggl entries are already in JIRA and have no differences - make no changes",
			sourceEntries: new[]
			{
				new WorkLogEntry("some-issue-key", "123", Now, 90, "Time Entry 1"),
				new WorkLogEntry("some-issue-key", "456", Now, 90, "Time Entry 2"),
			},
			targetEntries: new[]
			{
				new WorkLogEntry("some-issue-key", "123", Now, 90, "Time Entry 1"),
				new WorkLogEntry("some-issue-key", "456", Now, 90, "Time Entry 2"),
			},
			doPurge: true,
			expectedResult: new SyncPlan
			{
				NoChanges = new[]
				{
					new WorkLogEntry("some-issue-key", "123", Now, 90, "Time Entry 1"),
					new WorkLogEntry("some-issue-key", "456", Now, 90, "Time Entry 2"),
				}.ToList(),
			});

		yield return new SyncPlanScenario("Purge ON: If Toggl entries are already in JIRA but have differences - update JIRA entries based on Toggl entries",
			sourceEntries: new[]
			{
				new WorkLogEntry("some-issue-key", "123", Now, 90, "Toggl Description 1"),
				new WorkLogEntry("some-issue-key", "456", Now, 90, "Toggl Description 2"),
			},
			targetEntries: new[]
			{
				new WorkLogEntry("some-issue-key", "123", Now, 90, "JIRA Descr 1"),
				new WorkLogEntry("some-issue-key", "456", Now, 90, "JIRA Descr 2"),
			},
			doPurge: true,
			expectedResult: new SyncPlan
			{
				ToUpdate = new[]
				{
					new WorkLogEntry("some-issue-key", "123", Now, 90, "Toggl Description 1"),
					new WorkLogEntry("some-issue-key", "456", Now, 90, "Toggl Description 2"),
				}.ToList(),
			});

		yield return new SyncPlanScenario("Purge ON: If Toggl entry is already in JIRA but since then issue key was modified in toggl - remove old workLog, and add new one",
			sourceEntries: new[]
			{
				new WorkLogEntry("ABC-456", "123", Now, 90, ""),
			},
			targetEntries: new[]
			{
				new WorkLogEntry("ABC-123", "123", Now, 90, ""),
			},
			doPurge: true,
			expectedResult: new SyncPlan
			{
				ToDeleteOrphaned = new[]
				{
					new WorkLogEntry("ABC-123", "123", Now, 90, ""),
				}.ToList(),
				ToAdd = new[] { new WorkLogEntry("ABC-456", "123", Now, 90, "") }.ToList(),
			});
	}

	[TestCaseSource(nameof(CreateSyncPlanTestCases))]
	public void CreateSyncPlan(WorkLogEntry[] sourceEntries, WorkLogEntry[] targetEntries, bool doPurge, SyncPlan expectedResult)
	{
		var result = WorksheetSyncService.CreateSyncPlan(sourceEntries, targetEntries, doPurge);

		result.Should().BeEquivalentTo(expectedResult);
	}

	public static IEnumerable ApplyTestCases()
	{
		yield return new TestCaseData(new SyncPlan { ToAdd = new[] { new WorkLogEntry("some-issue-key", "source-id", Now, 90, "some-description"), new WorkLogEntry("some-issue-key", "source-id", Now, 90, "some-description") }.ToList() }, true)
			.SetName("Have new entries to ADD and AGREED, then should ADD");
		yield return new TestCaseData(new SyncPlan { ToAdd = new[] { new WorkLogEntry("some-issue-key", "source-id", Now, 90, "some-description"), new WorkLogEntry("some-issue-key", "source-id", Now, 90, "some-description") }.ToList() }, false)
			.SetName("Have new entries to ADD but NOT AGREED, then should NOT ADD");
		yield return new TestCaseData(new SyncPlan { ToAdd = new[] { new WorkLogEntry("some-issue-key", "source-id", Now, 90, "some-description"), new WorkLogEntry("some-issue-key", "source-id", Now, 90, "some-description") }.ToList() }, true)
			.SetName("Have changed entries to UPDATE and AGREED, then should UPDATE");
		yield return new TestCaseData(new SyncPlan { ToAdd = new[] { new WorkLogEntry("some-issue-key", "source-id", Now, 90, "some-description"), new WorkLogEntry("some-issue-key", "source-id", Now, 90, "some-description") }.ToList() }, false)
			.SetName("Have changed entries to UPDATE but NOT AGREED, then should NOT UPDATE");
		yield return new TestCaseData(new SyncPlan { ToDeleteDuplicates = new[] { new WorkLogEntry("some-issue-key", "source-id", Now, 90, "some-description"), new WorkLogEntry("some-issue-key", "source-id", Now, 90, "some-description") }.ToList() }, true)
			.SetName("Have duplicate entries to DELETE and AGREED, then should DELETE");
		yield return new TestCaseData(new SyncPlan { ToDeleteDuplicates = new[] { new WorkLogEntry("some-issue-key", "source-id", Now, 90, "some-description"), new WorkLogEntry("some-issue-key", "source-id", Now, 90, "some-description") }.ToList() }, false)
			.SetName("Have duplicate entries to DELETE but NOT AGREED, then should NOT DELETE");
		yield return new TestCaseData(new SyncPlan { ToDeleteOrphaned = new[] { new WorkLogEntry("some-issue-key", "source-id", Now, 90, "some-description"), new WorkLogEntry("some-issue-key", "source-id", Now, 90, "some-description") }.ToList() }, true)
			.SetName("Have orphaned entries to DELETE and AGREED, then should DELETE");
		yield return new TestCaseData(new SyncPlan { ToDeleteOrphaned = new[] { new WorkLogEntry("some-issue-key", "source-id", Now, 90, "some-description"), new WorkLogEntry("some-issue-key", "source-id", Now, 90, "some-description") }.ToList() }, false)
			.SetName("Have orphaned entries to DELETE but NOT AGREED, then should NOT DELETE");
		yield return new TestCaseData(new SyncPlan { NoChanges = new[] { new WorkLogEntry("some-issue-key", "source-id", Now, 90, "some-description"), new WorkLogEntry("some-issue-key", "source-id", Now, 90, "some-description") }.ToList() }, false)
			.SetName("Entries that didn't require any changes, should stay unchanged");
	}

	[TestCaseSource(nameof(ApplyTestCases))]
	public async Task Apply(SyncPlan syncPlan, bool isAgreed)
	{
		var source = Substitute.For<IExternalWorksheetRepository>();
		var target = Substitute.For<IJiraRepository>();
		var sut = new WorksheetSyncService(source,
			target,
			Options.Create(new WorksheetSyncService.Options
			{
				AgreeToAdd = _ => isAgreed,
				AgreeToUpdate = _ => isAgreed,
				AgreeToDeleteDuplicates = _ => isAgreed,
				AgreeToDeleteOrphaned = _ => isAgreed,
			}));

		var report = await sut.ApplyAsync(syncPlan);

		// assert that appropriate operation took place only if agreed
		await target.Received(isAgreed ? syncPlan.ToAdd.Count : 0).AddWorkLogAsync(Arg.Any<WorkLogEntry>());
		await target.Received(isAgreed ? syncPlan.ToUpdate.Count : 0).UpdateWorkLogAsync(Arg.Any<WorkLogEntry>());
		await target.Received(isAgreed ? syncPlan.ToDeleteDuplicates.Count + syncPlan.ToDeleteOrphaned.Count : 0).DeleteWorkLogAsync(Arg.Any<WorkLogEntry>());

		// assert report reflect the numbers
		Assert.That(report.AddedEntries.Count, Is.EqualTo(isAgreed ? syncPlan.ToAdd.Count : 0));
		Assert.That(report.UpdatedEntries.Count, Is.EqualTo(isAgreed ? syncPlan.ToUpdate.Count : 0));
		Assert.That(report.DeletedDuplicateEntries.Count, Is.EqualTo(isAgreed ? syncPlan.ToDeleteDuplicates.Count : 0));
		Assert.That(report.DeletedOrphanedEntries.Count, Is.EqualTo(isAgreed ? syncPlan.ToDeleteOrphaned.Count : 0));
		Assert.That(report.NoChanges.Count, Is.EqualTo(syncPlan.NoChanges.Count));
	}
}

public class SyncPlanScenario : TestCaseData
{
	public SyncPlanScenario(
		string testName,
		IEnumerable<WorkLogEntry> sourceEntries,
		IEnumerable<WorkLogEntry> targetEntries,
		bool doPurge,
		SyncPlan expectedResult
	)
		: base(sourceEntries, targetEntries, doPurge, expectedResult)
	{
		SetName(testName);
	}
}
