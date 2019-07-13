using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using JiraTogglSync.Services;
using NSubstitute;
using NUnit.Framework;
using NUnit.Framework.Constraints;
using Toggl;
using Toggl.Interfaces;
using Toggl.QueryObjects;

namespace JiraTogglSync.Tests
{
	public class TogglRepositoryTests
	{

		//         Description                  JIRA Project Keys        
		[TestCase("KEY-123 Doing some work",    new string[] { },        ExpectedResult = null,      TestName = "Should not match if keys are not provided")]
		[TestCase("KEY-123 Doing some work",    new[] { "KEY" },         ExpectedResult = "KEY-123", TestName = "Should match if one key matches exactly")]
		[TestCase("Doing some work KEY-123",    new[] { "LOCK", "KEY" }, ExpectedResult = "KEY-123", TestName = "Should match if one of the keys matches")]
		[TestCase("[KEY-123] Doing some work",  new[] { "LOCK", "DOOR" },ExpectedResult = null,      TestName = "Should not match of non of the keys match")]
		[TestCase("[KEY-123] [DOOR-345] work",  new[] { "KEY", "DOOR" }, ExpectedResult = "KEY-123", TestName = "Should match first key if more than key matches")]
		[TestCase("[MYKEY-123] some work",      new[] { "KEY" },         ExpectedResult = null,      TestName = "Should not match partial keys")]
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
					expectedResult: new WorkLogEntry[0]
					);

			yield return new GetEntriesScenario(
					testName: "If toggl entry has no description, then this entry should be excluded",
					rawOutputFromToggl: new[] { new TimeEntry { Description = null, Stop = DateTime.Now.ToString() } },
					jiraProjectKeys: new[] { "TEST" },
					expectedResult: new WorkLogEntry[0]
					);

			yield return new GetEntriesScenario(
					testName: "If toggl entry has issue key in description, then entry should be returned and this key should be extracted",
					rawOutputFromToggl: new[] { new TimeEntry { Description = "Working on [TEST-45] again", Start = "2000-1-1", Stop = "2000-1-2" } },
					jiraProjectKeys: new[] { "TEST" },
					expectedResult: new[] { new WorkLogEntry() { IssueKey = "TEST-45", Description = "Working on [TEST-45] again", Start = new DateTime(2000, 1, 1), Stop = new DateTime(2000, 1, 2) } }
			);

			yield return new GetEntriesScenario(
					testName: "If toggl entry does not have issue key in description, then entry should not be returned",
					rawOutputFromToggl: new[] { new TimeEntry { Description = "Working on [OTHER-45] again", Start = "2000-1-1", Stop = "2000-1-2" } },
					jiraProjectKeys: new[] { "TEST" },
					expectedResult: new WorkLogEntry[0]
			);
		}

		[TestCaseSource(nameof(GetEntriesTestCases))]
		public void GetEntriesTests(TimeEntry[] rawOutputFromToggl, string[] jiraProjectKeys, WorkLogEntry[] expectedResult)
		{
			var userService = Substitute.For<IUserService>();
			var timeEntryService = Substitute.For<ITimeEntryService>();
			timeEntryService.List(Arg.Any<TimeEntryParams>()).Returns(rawOutputFromToggl.ToList());

			var sut = new TogglRepository(timeEntryService, userService, descriptionTemplate: "{{toggl:description}}");

			var startDate = DateTime.Now; //actual value is irrelevent
			var endDate = DateTime.Now; //actual value is irrelevent

			var result = sut.GetEntries(startDate, endDate, jiraProjectKeys);

			result.Should().BeEquivalentTo(expectedResult);
		}

		public class GetEntriesScenario : TestCaseData
		{
			public GetEntriesScenario(
					string testName,
					TimeEntry[] rawOutputFromToggl,
					string[] jiraProjectKeys,
					WorkLogEntry[] expectedResult
					) : base(rawOutputFromToggl, jiraProjectKeys, expectedResult)
			{
				this.SetName(testName);
			}
		}

	}
}
