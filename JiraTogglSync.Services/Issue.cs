using System.Collections.Generic;

namespace JiraTogglSync.Services
{
	public class Issue
	{
		public string Key { get; set; }
		public string Summary { get; set; }

		public List<WorkLogEntry> WorkLog { get; set; }

		public Issue()
		{
			WorkLog = new List<WorkLogEntry>();
		}

		public override string ToString()
		{
			return string.Format("{0}: {1}", Key, Summary);
		}
	}
}