using System.Collections.Generic;

namespace JiraTogglSync.Services
{
	public interface IWorksheetTargetService
	{
		IEnumerable<Issue> LoadIssues(IEnumerable<string> keys);
		void AddWorkLog(WorkLogEntry entry);
	}
}