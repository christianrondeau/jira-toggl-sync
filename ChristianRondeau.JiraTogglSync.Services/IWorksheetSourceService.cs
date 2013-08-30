using System;
using System.Collections.Generic;

namespace ChristianRondeau.JiraTogglSync.Services
{
	public interface IWorksheetSourceService
	{
		IEnumerable<WorkLogEntry> GetEntries(DateTime startDate, DateTime endDate);
	}
}