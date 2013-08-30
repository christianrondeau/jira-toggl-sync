using System;
using System.Collections.Generic;

namespace ChristianRondeau.JiraTogglSync.Services
{
	public class WorksheetSyncService
	{
		private readonly IWorksheetSourceService _source;
		private readonly IWorksheetTargetService _target;

		public WorksheetSyncService(IWorksheetSourceService source, IWorksheetTargetService target)
		{
			_source = source;
			_target = target;
		}

		public IEnumerable<WorkLogEntry> GetSuggestions(DateTime fromDate, DateTime toDate)
		{
			return _source.GetEntries(fromDate, toDate);
		}
	}
}