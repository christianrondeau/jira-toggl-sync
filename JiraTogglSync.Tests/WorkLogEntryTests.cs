using System;
using JiraTogglSync.Services;
using NUnit.Framework;

namespace JiraTogglSync.Tests;

public class WorkLogEntryTests
{
	[Test]
	public void CanDisplayNicelyAsString()
	{
		var workLogEntryDate = new DateTime(2014, 03, 25);
		var dayAgo = Math.Ceiling((DateTime.Now - workLogEntryDate).TotalDays);
		Assert.That(
			$"[some-issue-key] - {workLogEntryDate:d} ({dayAgo}d ago) - 01:30:00 - My Entry",
			Is.EqualTo(new WorkLogEntry("some-issue-key", "some-source-id", workLogEntryDate, 90, "My Entry").ToString())
		);
	}

	[TestCase(null, ExpectedResult = null, TestName = "Sould return null, if input is null")]
	[TestCase("", ExpectedResult = null, TestName = "Sould return null, if input is empty")]
	[TestCase("Some random text", ExpectedResult = null, TestName = "Sould return null, if input doesn't have a match")]
	[TestCase("No source id value [toggl-id:]", ExpectedResult = null, TestName = "Sould return null, if value of toggl id is not provided")]
	[TestCase("Alpha source id value [toggl-id:123a]", ExpectedResult = null, TestName = "Sould return null, if value of toggle id contains non-numeric characters")]
	[TestCase("Source id has value [toggl-id:123] !", ExpectedResult = "123", TestName = "Sould return toggle id value if it was provided")]
	[TestCase("Multiple source id values [toggl-id:123] [toggl-id:456]!", ExpectedResult = "123", TestName = "Sould return first toggle id value is multiple are provided")]
	public string GetSourceId(string input)
	{
		return WorkLogEntry.GetSourceId(input);
	}
}
