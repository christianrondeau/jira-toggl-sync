using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using TechTalk.JiraRestClient;

namespace JiraTogglSync.Services
{
	public class WorksheetSyncService
	{
		private readonly IExternalWorksheetRepository _source;
		private readonly IJiraRepository _target;

		public WorksheetSyncService(IExternalWorksheetRepository source, IJiraRepository target)
		{
			_source = source;
			_target = target;

		}

		public IEnumerable<Issue> GetSuggestions(DateTime fromDate, DateTime toDate)
		{
			try
			{
                //TODO call properly
                var sourceEntries = _source.GetEntries(fromDate, toDate, null).ToList();

//				foreach (var entry in sourceEntries)
//				{
//					entry.IssueKey = ExtractJiraIncidentNumber(entry.Description);
//				}

				var validEntries = sourceEntries.Where(entry => entry.IssueKey != null);

				var issueKeys = validEntries.Select(entry => entry.IssueKey).Distinct();
				var issues = _target.LoadIssues(issueKeys).ToList();

				foreach (var issue in issues)
				{
					issue.WorkLog.AddRange(sourceEntries.Where(entry => entry.IssueKey == issue.Key));
				}

				return issues;
			}
			catch (JiraClientException ex)
			{
				throw new Exception($"{ex.Message}\n{ex.ErrorResponse}");
			}
		}

	    public SyncPlan GetSyncPlan(DateTime fromDate, DateTime toDate, IEnumerable<string> jiraProjectKeys)
	    {
	        var _sourceEntries = _source.GetEntries(fromDate, toDate, jiraProjectKeys);
	        //var _targetEntries = _target.LoadIssues();

            throw new NotImplementedException();
	    }

		public void AddWorkLog(WorkLogEntry entry)
		{
			_target.AddWorkLog(entry);
		}
	}
}