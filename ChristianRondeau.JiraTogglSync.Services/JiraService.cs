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

		public IEnumerable<Issue> LoadIncidents(IEnumerable<string> keys)
		{
			return _jira
				.GetIssuesFromJql(string.Format("key in ({0})", string.Join(",", keys)))
				.Select(ConvertToIncident);
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
