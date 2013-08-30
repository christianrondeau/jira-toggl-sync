using System.Collections.Generic;

namespace ChristianRondeau.JiraTogglSync.Services
{
	public interface IWorksheetTargetService
	{
		IEnumerable<Issue> LoadIncidents(IEnumerable<string> keys);
	}
}