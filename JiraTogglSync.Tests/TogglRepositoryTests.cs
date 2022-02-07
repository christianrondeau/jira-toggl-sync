using System;
using System.Collections;
using System.Linq;
using FluentAssertions;
using JiraTogglSync.Services;
using Microsoft.Extensions.Options;
using NSubstitute;
using NUnit.Framework;
using Toggl.Api.DataObjects;
using Toggl.Api.Interfaces;
using Toggl.Api.QueryObjects;
using Task = System.Threading.Tasks.Task;

namespace JiraTogglSync.Tests;

public class TogglRepositoryTests
{
	//         Description                  JIRA Project Keys
	[TestCase("KEY-123 Doing some work", new string[] { }, ExpectedResult = null, TestName = "Should not match if keys are not provided")]
	[TestCase("KEY-123 Doing some work", new[] { "KEY" }, ExpectedResult = "KEY-123", TestName = "Should match if one key matches exactly")]
	[TestCase("Doing some work KEY-123", new[] { "LOCK", "KEY" }, ExpectedResult = "KEY-123", TestName = "Should match if one of the keys matches")]
	[TestCase("[KEY-123] Doing some work", new[] { "LOCK", "DOOR" }, ExpectedResult = null, TestName = "Should not match of non of the keys match")]
	[TestCase("[KEY-123] [DOOR-345] work", new[] { "KEY", "DOOR" }, ExpectedResult = "KEY-123", TestName = "Should match first key if more than key matches")]
	[TestCase("[MYKEY-123] some work", new[] { "KEY" }, ExpectedResult = null, TestName = "Should not match partial keys")]
	public string ExtractIssueKeyTests(string description, string[] jiraProjectKeys)
	{
		var result = TogglRepository.ExtractIssueKey(description, jiraProjectKeys.ToList());
		return result;
	}

	public static IEnumerable GetEntriesTestCases()
	{
		yield return new GetEntriesScenario(
			testName: "If toggl entry is still active (not stopped), then this entry should be excluded",
			rawOutputFromToggl: new[] { new TimeEntry { Description = "Still keeping track of time", Stop = null } },
			jiraProjectKeys: new[] { "TEST" },
			expectedResult: Array.Empty<WorkLogEntry>()
		);

		yield return new GetEntriesScenario(
			testName: "If toggl entry has no description, then this entry should be excluded",
			rawOutputFromToggl: new[] { new TimeEntry { Description = null, Stop = DateTime.UtcNow.ToString("O") } },
			jiraProjectKeys: new[] { "TEST" },
			expectedResult: new WorkLogEntry[0]
		);

		yield return new GetEntriesScenario(
			testName: "If toggl entry has issue key in description, then entry should be returned and this key should be extracted",
			rawOutputFromToggl: new[] { new TimeEntry { Id = 42, Description = "Working on [TEST-45] again", Start = "2000-1-1", Stop = "2000-1-2" } },
			jiraProjectKeys: new[] { "TEST" },
			expectedResult: new[] { new WorkLogEntry("TEST-45", "42", new DateTime(2000, 1, 1), 24*60, "Working on [TEST-45] again") }
		);

		yield return new GetEntriesScenario(
			testName: "If toggl entry does not have issue key in description, then entry should not be returned",
			rawOutputFromToggl: new[] { new TimeEntry { Description = "Working on [OTHER-45] again", Start = "2000-1-1", Stop = "2000-1-2" } },
			jiraProjectKeys: new[] { "TEST" },
			expectedResult: new WorkLogEntry[0]
		);
	}

	[TestCaseSource(nameof(GetEntriesTestCases))]
	public async Task GetEntriesTests(TimeEntry[] rawOutputFromToggl, string[] jiraProjectKeys, WorkLogEntry[] expectedResult)
	{
		var userService = Substitute.For<IUserServiceAsync>();
		var timeEntryService = Substitute.For<ITimeEntryServiceAsync>();

		timeEntryService.GetAllAsync(Arg.Any<TimeEntryParams>())
			.Returns(rawOutputFromToggl.ToList());

		var options = new TogglRepository.Options
		{
			DescriptionTemplate = "{{toggl:description}}"
		};
		var sut = new TogglRepository(timeEntryService, userService, Options.Create(options), new TimeUtil());

		var startDate = DateTime.Now; //actual value is irrelevent
		var endDate = DateTime.Now; //actual value is irrelevent

		var result = await sut.GetEntriesAsync(startDate, endDate, jiraProjectKeys, 5);

		result.Should().BeEquivalentTo(expectedResult);
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