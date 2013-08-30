using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace ChristianRondeau.JiraTogglSync.Services
{
	public class WorksheetSyncService
	{
		private readonly IList<Regex> _regexes;
		private readonly IWorksheetSourceService _source;
		private readonly IWorksheetTargetService _target;

		public WorksheetSyncService(IWorksheetSourceService source, IWorksheetTargetService target, IEnumerable<string> prefixes)
		{
			_source = source;
			_target = target;

			_regexes = prefixes.Select(prefix => new Regex(prefix + @"-\d+")).ToList();
		}

		public IEnumerable<Issue> GetSuggestions(DateTime fromDate, DateTime toDate)
		{
			var sourceEntries = _source.GetEntries(fromDate, toDate).ToList();

			foreach (var entry in sourceEntries)
			{
				entry.IssueKey = ExtractJiraIncidentNumber(entry.Description);
				entry.Description = RemoveIncidentNumber(entry.Description, entry.IssueKey);
			}

			var validEntries = sourceEntries.Where(entry => entry.IssueKey != null);

			var issues = _target.LoadIssues(validEntries.Select(entry => entry.IssueKey).Distinct()).ToList();

			foreach (var issue in issues)
			{
				issue.WorkLog.AddRange(sourceEntries.Where(entry => entry.IssueKey == issue.Key));
			}
			
			return issues;
		}

		private static string RemoveIncidentNumber(string description, string issueKey)
		{
			return new Regex("^" + issueKey + @"[\s:]+").Replace(description, "");
		}

		private string ExtractJiraIncidentNumber(string description)
		{
			return _regexes
				.Select(regex => regex.Match(description))
				.Where(x => x.Success)
				.Select(x => x.Value)
				.FirstOrDefault();
		}

		public void AddWorkLog(WorkLogEntry entry)
		{
			_target.AddWorkLog(entry);
		}
	}
}