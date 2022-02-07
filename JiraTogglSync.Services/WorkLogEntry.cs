using System;
using System.Text.RegularExpressions;
using Atlassian.Jira;

namespace JiraTogglSync.Services;

public class WorkLogEntry
{
	public string IssueKey { get; set; }
	public Worklog JiraWorkLog { get; }

	public string SourceId { get; set; }

	public TimeSpan TimeSpent => JiraWorkLog.TimeSpentInSeconds > 0 ? TimeSpan.FromSeconds(JiraWorkLog.TimeSpentInSeconds) : TimeSpan.FromMinutes(int.Parse(JiraWorkLog.TimeSpent.Split("m")[0]));

	public static string? GetSourceId(string? description)
	{
		var regex = new Regex(@"\[toggl-id:(?<sourceId>[0-9]+)]");
		var matchResult = regex.Match(description ?? "");
		return matchResult.Success ? matchResult.Groups["sourceId"].Value : null;
	}

	public WorkLogEntry(string issueKey, string sourceId, DateTime startDate, int timeSpentInMinutes, string description)
	{
		IssueKey = issueKey;
		SourceId = sourceId;
		JiraWorkLog = new Worklog(timeSpentInMinutes + "m", startDate, description);
	}

	public WorkLogEntry(string issueKey, string sourceId, Worklog jiraWorkLog)
	{
		IssueKey = issueKey;
		SourceId = sourceId;
		JiraWorkLog = jiraWorkLog;
	}

	public override string ToString()
	{
		return $"[{IssueKey}] - {JiraWorkLog.StartDate:d} - {TimeSpent} - {JiraWorkLog.Comment}";
	}

	public bool DifferentFrom(WorkLogEntry other)
	{
		if (other.TimeSpent != TimeSpent)
			return true;
		if (other.JiraWorkLog.StartDate != JiraWorkLog.StartDate)
			return true;
		if (other.JiraWorkLog.Comment != JiraWorkLog.Comment)
			return true;
		return false;
	}

	public void Synchronize(WorkLogEntry other)
	{
		JiraWorkLog.StartDate = other.JiraWorkLog.StartDate;
		JiraWorkLog.TimeSpent = other.JiraWorkLog.TimeSpent;
		JiraWorkLog.Comment = other.JiraWorkLog.Comment;
	}
}