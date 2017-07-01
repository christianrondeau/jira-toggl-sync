using System;
using System.Collections.Generic;
using System.Linq;
using TechTalk.JiraRestClient;

namespace JiraTogglSync.Services
{
	public class JiraRestService : IJiraRepository
	{
		private readonly string _username;

		private readonly IJiraClient<IssueFields> _jira;

		public JiraRestService(string instance, string username, string password)
		{
			_username = username;
			_jira = new JiraClient<IssueFields>(instance, username, password);
		}

		public IEnumerable<Issue> LoadIssues(IEnumerable<string> keys)
		{
			if (!keys.Any()) return Enumerable.Empty<Issue>();

			var jqlQuery = string.Format("key+in+({0})", string.Join(",", keys));
			return _jira.GetIssuesByQuery(jqlQuery)
				.Select(ConvertToIncident);
		}

		public WorkLogEntry[] GetEntries(DateTime startDate, DateTime endDate, IEnumerable<string> jiraProjectKeys)
		{
			//Basically we need to find all work log items that have been created or edited within start and end dates.
			//Note: since we don't have endpoint for 'Get ids of worklogs modified since', we will find those work logs 
			//through issues that were recently modified.

			if (jiraProjectKeys == null || !jiraProjectKeys.Any())
				return new WorkLogEntry[0];

			var jqlQuery = string.Format("project in ({0}) AND updated >= {1} AND updated <= {2}",
				string.Join(", ", jiraProjectKeys),
				startDate.ToString("yyyy-MM-dd"),
				endDate.ToString("yyyy-MM-dd")
				);

			var recentlyUpdatedIssues = _jira.GetIssuesByQuery(jqlQuery);

			var result = new List<WorkLogEntry>();

			foreach (var issue in recentlyUpdatedIssues)
			{
				var workLogs = _jira.GetWorklogs(new IssueRef() { id = issue.id })
					.Where(workLog => workLog.updated >= startDate 
									&& workLog.updated <= endDate 
									&& workLog?.author?.name == _username);

				result.AddRange(workLogs.Select(wl => new WorkLogEntry(wl, issue.key)));
			}

			return result.ToArray();
		}


		public OperationResult UpdateWorkLog(WorkLogEntry entry)
		{
			try
			{
				_jira.UpdateWorklog(
					new IssueRef() { id = entry.IssueKey },
					new Worklog()
					{
						id = entry.Id,
						comment = entry.Description,
						started = entry.Start,
						timeSpentSeconds = (int)entry.RoundedDuration.TotalSeconds
					});
				return OperationResult.Success(entry);
			}
			catch (Exception ex)
			{
				return OperationResult.Error(ex.Message, entry);
			}
		}

		public OperationResult DeleteWorkLog(WorkLogEntry entry)
		{
			try
			{
				_jira.DeleteWorklog(new IssueRef() { id = entry.IssueKey }, new Worklog() { id = entry.Id });
				return OperationResult.Success(entry);
			}
			catch (Exception ex)
			{
				return OperationResult.Error(ex.Message, entry);
			}
		}

		public OperationResult AddWorkLog(WorkLogEntry entry)
		{
			try
			{
				var timeSpentSeconds = (int)entry.RoundedDuration.TotalSeconds;
				_jira.CreateWorklog(new IssueRef { id = entry.IssueKey }, timeSpentSeconds, entry.Description, entry.Start);
				return OperationResult.Success(entry);
			}
			catch (Exception ex)
			{
				return OperationResult.Error(ex.Message, entry);
			}
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
