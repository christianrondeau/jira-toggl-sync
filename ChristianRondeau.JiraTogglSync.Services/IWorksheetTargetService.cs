using System.Collections.Generic;

namespace ChristianRondeau.JiraTogglSync.Services
{
	public interface IWorksheetTargetService
	{
		IEnumerable<Issue> LoadIssues(IEnumerable<string> keys);
		void AddWorkLog(WorkLogEntry entry);
	}
}