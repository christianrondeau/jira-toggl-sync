using System.Collections.Generic;

namespace JiraTogglSync.Services
{
	public interface IJiraRepository
	{
		IEnumerable<Issue> LoadIssues(IEnumerable<string> keys);
		void AddWorkLog(WorkLogEntry entry);
	}
}