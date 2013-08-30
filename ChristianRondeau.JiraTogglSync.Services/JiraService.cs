using System;
using System.Collections.Generic;
using System.Linq;
using Atlassian.Jira;

namespace ChristianRondeau.JiraTogglSync.Services
{
	public class JiraService : IWorksheetTargetService
    {
        private readonly Jira _jira;

        public JiraService(string instance, string username, string password)
        {
            _jira = new Jira(instance, username, password);
        }

        public string GetUserInformation()
        {
            return _jira.Url;
        }

		public IEnumerable<Issue> LoadIssues(IEnumerable<string> keys)
		{
			return _jira
				.GetIssuesFromJql(string.Format("key in ({0})", string.Join(",", keys)))
				.Select(ConvertToIncident);
		}

		public void AddWorkLog(WorkLogEntry entry)
		{
			var timeSpent = (entry.Stop - entry.Start);
			string timeSpentString;
			if (timeSpent.TotalMinutes > 60)
				timeSpentString = Math.Round(timeSpent.TotalHours).ToString("0") + "h";
			else
				timeSpentString = Math.Max(1, Math.Round(timeSpent.TotalMinutes)).ToString("0") + "m";

			_jira.GetIssue(entry.IssueKey).AddWorklog(
				new Worklog(
					timeSpentString,
					entry.Start,
					entry.Description
					),
					WorklogStrategy.RetainRemainingEstimate
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
