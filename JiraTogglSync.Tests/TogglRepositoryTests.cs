using System;
using System.Collections.Generic;
using System.Linq;
using JiraTogglSync.Services;
using NUnit.Framework;
using Toggl.Api.Models;

namespace JiraTogglSync.Tests;

public class TogglRepositoryTests
{
	//         Description                  JIRA Project Keys
	[TestCase("KEY-123 Doing some work", new string[] { }, ExpectedResult = null, TestName = "Should not match if keys are not provided")]
	[TestCase("KEY-123 Doing some work", new[] {"KEY"}, ExpectedResult = "KEY-123", TestName = "Should match if one key matches exactly")]
	[TestCase("Doing some work KEY-123", new[] {"LOCK", "KEY"}, ExpectedResult = "KEY-123", TestName = "Should match if one of the keys matches")]
	[TestCase("[KEY-123] Doing some work", new[] {"LOCK", "DOOR"}, ExpectedResult = null, TestName = "Should not match of non of the keys match")]
	[TestCase("[KEY-123] [DOOR-345] work", new[] {"KEY", "DOOR"}, ExpectedResult = "KEY-123", TestName = "Should match first key if more than key matches")]
	[TestCase("[MYKEY-123] some work", new[] {"KEY"}, ExpectedResult = null, TestName = "Should not match partial keys")]
	public string ExtractIssueKeyTests(string description, string[] jiraProjectKeys)
	{
		var result = TogglRepository.ExtractIssueKey(description, jiraProjectKeys.ToList());
		return result;
	}

	public static IEnumerable<GetEntriesScenario> GetEntriesTestCases()
	{
		yield return new GetEntriesScenario(
			testName: "If toggl entry is still active (not stopped), then this entry should be excluded",
			rawOutputFromToggl:
			[
				CreateTimeEntry(t =>
					{
						t.Description = "Still keeping track of time";
						t.Stop = null;
					}
				),
			],
			jiraProjectKeys: ["TEST"],
			expectedResult: []
		);

		yield return new GetEntriesScenario(
			testName: "If toggl entry has no description, then this entry should be excluded",
			rawOutputFromToggl:
			[
				CreateTimeEntry(t =>
					{
						t.Description = null;
						t.Stop = DateTimeOffset.UtcNow;
					}
				),
			],
			jiraProjectKeys: ["TEST"],
			expectedResult: []
		);

		yield return new GetEntriesScenario(
			testName: "If toggl entry has issue key in description, then entry should be returned and this key should be extracted",
			rawOutputFromToggl:
			[
				CreateTimeEntry(t =>
					{
						t.Id = 42;
						t.Description = "Working on [TEST-45] again";
						t.Start = new DateTimeOffset(2020, 1, 1, 0, 0, 0, TimeSpan.Zero);
						t.Stop = new DateTimeOffset(2020, 1, 2, 0, 0, 0, TimeSpan.Zero);
					}
				),
			],
			jiraProjectKeys: ["TEST"],
			expectedResult: [new WorkLogEntry("TEST-45", "42", new DateTime(2000, 1, 1), 24 * 60, "Working on [TEST-45] again")]
		);

		yield return new GetEntriesScenario(
			testName: "If toggl entry does not have issue key in description, then entry should not be returned",
			rawOutputFromToggl:
			[
				CreateTimeEntry(t =>
					{
						t.Description = "Working on [OTHER-45] again";
						t.Start = new DateTimeOffset(2020, 1, 1, 0, 0, 0, TimeSpan.Zero);
						t.Stop = new DateTimeOffset(2020, 1, 2, 0, 0, 0, TimeSpan.Zero);
					}
				),
			],
			jiraProjectKeys: ["TEST"],
			expectedResult: []
		);
	}

	private static TimeEntry CreateTimeEntry(Action<TimeEntry> customize = null)
	{
		var timeEntry = new TimeEntry
		{
			At = default,
			Billable = false,
			Duration = 0,
			Duronly = false,
		};
		customize?.Invoke(timeEntry);
		return timeEntry;
	}

	[TestCaseSource(nameof(GetEntriesTestCases))]
	public void GetEntriesTests(TimeEntry[] rawOutputFromToggl, string[] jiraProjectKeys, WorkLogEntry[] expectedResult)
	{
		Assert.Inconclusive("New toggle client is not mockable");

		/*
		var togglClient = Substitute.For<TogglClient>([new TogglClientOptions {Key = "some-key"}]);
		var options = new TogglRepository.Options
		{
			DescriptionTemplate = "{{toggl:description}}",
		};

		var startDate = DateTimeOffset.UtcNow.AddDays(1);
		var endDate = DateTimeOffset.UtcNow.AddDays(2);

		togglClient.TimeEntries.GetAsync(true, false, null, null, startDate, endDate, CancellationToken.None)
			.Returns(rawOutputFromToggl.ToList());

		var sut = new TogglRepository(togglClient, Options.Create(options), new TimeUtil());
		var result = await sut.GetEntriesAsync(startDate, endDate, jiraProjectKeys, 5);

		result.Should().BeEquivalentTo(expectedResult);*/
	}

	public class GetEntriesScenario : TestCaseData
	{
		public GetEntriesScenario(
			string testName,
			TimeEntry[] rawOutputFromToggl,
			string[] jiraProjectKeys,
			WorkLogEntry[] expectedResult
		)
			: base(rawOutputFromToggl, jiraProjectKeys, expectedResult)
		{
			this.SetName(testName);
		}
	}
}