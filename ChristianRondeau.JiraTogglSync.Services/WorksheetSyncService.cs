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

			var jiraIncentKeys = sourceEntries.Select(ExtractJiraIncidentNumber).Where(x => x != null).Distinct();

			var incidents = _target.LoadIncidents(jiraIncentKeys);
			
			return incidents;
		}

		private string ExtractJiraIncidentNumber(WorkLogEntry arg)
		{
			return _regexes
				.Select(regex => regex.Match(arg.Description))
				.Where(x => x.Success)
				.Select(x => x.Value)
				.FirstOrDefault();
		}
	}
}