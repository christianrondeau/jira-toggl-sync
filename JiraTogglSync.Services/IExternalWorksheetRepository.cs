using System;
using System.Collections.Generic;

namespace JiraTogglSync.Services
{
	public interface IExternalWorksheetRepository
	{
		WorkLogEntry[] GetEntries(DateTime startDate, DateTime endDate, IEnumerable<string> jiraProjectKeys );
	}
}