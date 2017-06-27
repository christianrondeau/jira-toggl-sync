using System;
using System.Collections.Generic;

namespace JiraTogglSync.Services
{
	public interface IExternalWorksheetRepository
	{
		IEnumerable<WorkLogEntry> GetEntries(DateTime startDate, DateTime endDate, IEnumerable<string> jiraProjectKeys );
	}
}