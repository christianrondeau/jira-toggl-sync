using System;
using System.Collections.Generic;

namespace JiraTogglSync.Services
{
	public interface IWorksheetSourceService
	{
		IEnumerable<WorkLogEntry> GetEntries(DateTime startDate, DateTime endDate);
	}
}