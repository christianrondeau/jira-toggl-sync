using System;
using System.Collections.Generic;
using System.Linq;
using Atlassian.Jira;

namespace JiraTogglSync.Services
{
	public class JiraService : IWorksheetTargetService
    {
		private readonly Jira _jira;
		private readonly WorklogStrategy _worklogStrategy;

		public JiraService(string instance, string username, string password, string worklogStrategy)
        {
	        _jira = new Jira(instance, username, password);

			if (!Enum.TryParse(worklogStrategy, out _worklogStrategy))
				_worklogStrategy = WorklogStrategy.RetainRemainingEstimate;
        }

		public string GetUserInformation()
        {
            return _jira.Url;
        }

		public IEnumerable<Issue> LoadIssues(IEnumerable<string> keys)
		{
			if (!keys.Any()) return Enumerable.Empty<Issue>();

			return _jira
				.GetIssuesFromJql(string.Format("key in ({0})", string.Join(",", keys)))
				.Select(ConvertToIncident);
		}

		public void AddWorkLog(WorkLogEntry entry)
		{
			var timeSpentString = string.Format("{0}m", entry.RoundedDuration.TotalMinutes);

			_jira.GetIssue(entry.IssueKey).AddWorklog(
				new Worklog(
					timeSpentString,
					entry.Start,
					entry.Description
					),
					_worklogStrategy
				);
		}

		private static Issue ConvertToIncident(Atlassian.Jira.Issue issue)
		{
			return new Issue
				{
					Key = issue.Key.Value,
					Summary = issue.Summary
				};
		}
    }
}
