using System;
using System.Collections.Generic;

namespace JiraTogglSync.Services
{
	public interface IJiraRepository
	{
        WorkLogEntry[] GetEntries(DateTime startDate, DateTime endDate, IEnumerable<string> jiraProjectKeys);
        OperationResult AddWorkLog(WorkLogEntry entry);
	    OperationResult UpdateWorkLog(WorkLogEntry entry);
	    OperationResult DeleteWorkLog(WorkLogEntry entry);
	}
}