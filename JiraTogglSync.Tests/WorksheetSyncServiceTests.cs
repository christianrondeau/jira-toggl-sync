using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using FluentAssertions;
using JiraTogglSync.Services;
using NSubstitute;
using NUnit.Framework;

namespace JiraTogglSync.Tests
{
	public class WorksheetSyncServiceTests
	{
		public static IEnumerable CreateSyncPlanTestCases()
		{
			yield return new SyncPlanScenario("Purge OFF: If no Toggl entries - make no changes",
				sourceEntries: Enumerable.Empty<WorkLogEntry>(),
				targetEntries: Enumerable.Empty<WorkLogEntry>(),
				doPurge: false,
				expectedResult: new SyncPlan());

			yield return new SyncPlanScenario("Purge OFF: If there are Toggl entries - add them to JIRA",
				sourceEntries: new[] {new WorkLogEntry() {Description = "New Toggl entry"}},
				targetEntries: Enumerable.Empty<WorkLogEntry>(),
				doPurge: false,
				expectedResult: new SyncPlan() {ToAdd = new[] {new WorkLogEntry() {Description = "New Toggl entry"}}.ToList()});

			yield return new SyncPlanScenario("Purge ON: If there are entries in JIRA, but no Toggl entries - remove JIRA entries",
				sourceEntries: Enumerable.Empty<WorkLogEntry>(),
				targetEntries: new[] {new WorkLogEntry() {Description = "Existing JIRA entry"}},
				doPurge: true,
				expectedResult:
					new SyncPlan() {ToDeleteOrphaned = new[] {new WorkLogEntry() {Description = "Existing JIRA entry"}}.ToList()});

			yield return new SyncPlanScenario("Purge ON: If there are no JIRA nor Toggl entries - make no changes",
				sourceEntries: Enumerable.Empty<WorkLogEntry>(),
				targetEntries: Enumerable.Empty<WorkLogEntry>(),
				doPurge: true,
				expectedResult: new SyncPlan());

			yield return new SyncPlanScenario("Purge ON: If there are duplicate entries in JIRA (based on toggl ID) - remove duplicates JIRA entries and add new Toggl entry",
				sourceEntries: new[] {new WorkLogEntry() {Description = "Toggl Entry"}.SetSourceId(123)},
				targetEntries: new[]
				{
					new WorkLogEntry() {Description = "JIRA Dup 1"}.SetSourceId(123),
					new WorkLogEntry() {Description = "JIRA Dup 2"}.SetSourceId(123)
				},
				doPurge: true,
				expectedResult: new SyncPlan()
				{
					ToDeleteDuplicates = new[]
					{
						new WorkLogEntry() {Description = "JIRA Dup 1"}.SetSourceId(123),
						new WorkLogEntry() {Description = "JIRA Dup 2"}.SetSourceId(123)
					}.ToList(),
					ToAdd = new[] {new WorkLogEntry() {Description = "Toggl Entry"}.SetSourceId(123)}.ToList()
				});

			yield return new SyncPlanScenario("Purge ON: If there are new Toggl entries - add them to JIRA",
				sourceEntries: new[] {new WorkLogEntry() {Description = "New Toggl Entry"}},
				targetEntries: Enumerable.Empty<WorkLogEntry>(),
				doPurge: true,
				expectedResult: new SyncPlan() {ToAdd = new[] {new WorkLogEntry() {Description = "New Toggl Entry"}}.ToList()});

			yield return new SyncPlanScenario("Purge ON: If Toggl entries are already in JIRA and have no differences - make no changes",
				sourceEntries: new[]
				{
					new WorkLogEntry() {Description = "Time Entry 1"}.SetSourceId(123),
					new WorkLogEntry() {Description = "Time Entry 2"}.SetSourceId(456)
				},
				targetEntries: new[]
				{
					new WorkLogEntry() {Description = "Time Entry 1"}.SetSourceId(123),
					new WorkLogEntry() {Description = "Time Entry 2"}.SetSourceId(456)
				},
				doPurge: true,
				expectedResult: new SyncPlan()
				{
					NoChanges = new[]
					{
						new WorkLogEntry() {Description = "Time Entry 1"}.SetSourceId(123),
						new WorkLogEntry() {Description = "Time Entry 2"}.SetSourceId(456)
					}.ToList()
				});

			yield return new SyncPlanScenario("Purge ON: If Toggl entries are already in JIRA but have differences - update JIRA entries based on Toggl entries",
				sourceEntries: new[]
				{
					new WorkLogEntry() {Description = "Toggl Description 1"}.SetSourceId(123),
					new WorkLogEntry() {Description = "Toggl Description 2"}.SetSourceId(456)
				},
				targetEntries: new[]
				{
					new WorkLogEntry() {Description = "JIRA Descr 1"}.SetSourceId(123),
					new WorkLogEntry() {Description = "JIRA Descr 2"}.SetSourceId(456)
				},
				doPurge: true,
				expectedResult: new SyncPlan()
				{
					ToUpdate = new[]
					{
						new WorkLogEntry() {Description = "Toggl Description 1"}.SetSourceId(123),
						new WorkLogEntry() {Description = "Toggl Description 2"}.SetSourceId(456)
					}.ToList()
				});

			yield return new SyncPlanScenario("Purge ON: If Toggl entry is already in JIRA but since then issue key was modified in toggl - remove old workLog, and add new one",
				sourceEntries: new[]
				{
					new WorkLogEntry() {Description = "", IssueKey = "ABC-456"}.SetSourceId(123),
				},
				targetEntries: new[]
				{
					new WorkLogEntry() {Description = "", IssueKey = "ABC-123"}.SetSourceId(123),
				},
				doPurge: true,
				expectedResult: new SyncPlan()
				{
					ToDeleteOrphaned = new[] { new WorkLogEntry() {Description = "", IssueKey = "ABC-123"}.SetSourceId(123) }.ToList(),
					ToAdd = new[] { new WorkLogEntry() {Description = "", IssueKey = "ABC-456"}.SetSourceId(123) }.ToList()
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
			yield return new TestCaseData(
					new SyncPlan() { ToAdd = new[] { new WorkLogEntry(), new WorkLogEntry() }.ToList() },
					true).SetName("Have new entries to ADD and AGREED, then should ADD");

			yield return new TestCaseData(
					new SyncPlan() { ToAdd = new[] { new WorkLogEntry(), new WorkLogEntry() }.ToList() },
					false).SetName("Have new entries to ADD but NOT AGREED, then should NOT ADD");

			yield return new TestCaseData(
					new SyncPlan() { ToAdd = new[] { new WorkLogEntry(), new WorkLogEntry() }.ToList() },
					true).SetName("Have changed entries to UPDATE and AGREED, then should UPDATE");

			yield return new TestCaseData(
					new SyncPlan() { ToAdd = new[] { new WorkLogEntry(), new WorkLogEntry() }.ToList() },
					false).SetName("Have changed entries to UPDATE but NOT AGREED, then should NOT UPDATE");

			yield return new TestCaseData(
					new SyncPlan() { ToDeleteDuplicates = new[] { new WorkLogEntry(), new WorkLogEntry() }.ToList() },
					true).SetName("Have duplicate entries to DELETE and AGREED, then should DELETE");

			yield return new TestCaseData(
					new SyncPlan() { ToDeleteDuplicates = new[] { new WorkLogEntry(), new WorkLogEntry() }.ToList() },
					false).SetName("Have duplicate entries to DELETE but NOT AGREED, then should NOT DELETE");

			yield return new TestCaseData(
					new SyncPlan() { ToDeleteOrphaned = new[] { new WorkLogEntry(), new WorkLogEntry() }.ToList() },
					true).SetName("Have orphaned entries to DELETE and AGREED, then should DELETE");

			yield return new TestCaseData(
					new SyncPlan() { ToDeleteOrphaned = new[] { new WorkLogEntry(), new WorkLogEntry() }.ToList() },
					false).SetName("Have orphaned entries to DELETE but NOT AGREED, then should NOT DELETE");

			yield return new TestCaseData(
					new SyncPlan() { NoChanges = new[] { new WorkLogEntry(), new WorkLogEntry() }.ToList() },
					false).SetName("Entries that didn't require any changes, should stay unchanged");
		}

		[TestCaseSource(nameof(ApplyTestCases))]
		public void Apply(SyncPlan syncPlan, bool isAgreed)
		{
			var source = Substitute.For<IExternalWorksheetRepository>();
			var target = Substitute.For<IJiraRepository>();
			var sut = new WorksheetSyncService(source, target)
			{
				AgreeToAdd = (items) => isAgreed,
				AgreeToUpdate = (items) => isAgreed,
				AgreeToDeleteDuplicates = (items) => isAgreed,
				AgreeToDeleteOrphaned = (items) => isAgreed
			};

			var report = sut.Apply(syncPlan);

			//assert that appropriate operation took place only if agreed
			target.Received(isAgreed ? syncPlan.ToAdd.Count : 0).AddWorkLog(Arg.Any<WorkLogEntry>());
			target.Received(isAgreed ? syncPlan.ToUpdate.Count : 0).UpdateWorkLog(Arg.Any<WorkLogEntry>());
			target.Received(isAgreed ? syncPlan.ToDeleteDuplicates.Count + syncPlan.ToDeleteOrphaned.Count : 0).DeleteWorkLog(Arg.Any<WorkLogEntry>());

			//sert report reflect the numbers
			Assert.AreEqual(report.AddedEntries.Count, isAgreed ? syncPlan.ToAdd.Count : 0);
			Assert.AreEqual(report.UpdatedEntries.Count, isAgreed ? syncPlan.ToUpdate.Count : 0);
			Assert.AreEqual(report.DeletedDuplicateEntries.Count, isAgreed ? syncPlan.ToDeleteDuplicates.Count : 0);
			Assert.AreEqual(report.DeletedOrphanedEntries.Count, isAgreed ? syncPlan.ToDeleteOrphaned.Count : 0);
			Assert.AreEqual(report.NoChanges.Count, syncPlan.NoChanges.Count);
		}

	}

	public static class WorkLogEntryExtensions
	{
		public static WorkLogEntry SetSourceId(this WorkLogEntry workLogEntry, int sourceId)
		{
			workLogEntry.Description = $"[toggl-id:{sourceId}] " + workLogEntry.Description;
			return workLogEntry;
		}
	}

	public class SyncPlanScenario : TestCaseData
	{
		public string MyProperty { get; set; }
		public SyncPlanScenario(
				string testName,
				IEnumerable<WorkLogEntry> sourceEntries,
				IEnumerable<WorkLogEntry> targetEntries,
				bool doPurge,
				SyncPlan expectedResult
				) : base(sourceEntries, targetEntries, doPurge, expectedResult)
		{
			this.SetName(testName);
		}
	}

}
