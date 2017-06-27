using System;
using System.Collections.Generic;
using System.Linq;
using TechTalk.JiraRestClient;

namespace JiraTogglSync.Services
{
	public class JiraRestService : IJiraRepository
    {
		
		private readonly IJiraClient<IssueFields> _jira;

		public JiraRestService(string instance, string username, string password)
        {
	        _jira = new JiraClient<IssueFields>(instance, username, password);
        }

		public IEnumerable<Issue> LoadIssues(IEnumerable<string> keys)
		{
			if (!keys.Any()) return Enumerable.Empty<Issue>();

			var jqlQuery = string.Format("key+in+({0})", string.Join(",", keys));
			return _jira.GetIssuesByQuery(jqlQuery)
				.Select(ConvertToIncident);
		}

		public void AddWorkLog(WorkLogEntry entry)
		{
			var timeSpentSeconds = (int)entry.RoundedDuration.TotalSeconds;

			_jira.CreateWorklog(new IssueRef {id = entry.IssueKey}, timeSpentSeconds, entry.Description, entry.Start);
		}

		private static Issue ConvertToIncident(Issue<IssueFields> issue)
		{
			return new Issue
				{
					Key = issue.key,
					Summary = issue.fields.summary
				};
		}

		public string GetUserInformation()
		{
			return _jira.GetLoggedInUser().self;
		}
    }
}
