using System;

namespace ChristianRondeau.JiraTogglSync.Services
{
	public class WorkLogEntry
	{
		public string IssueKey { get; set; }
		public string Description { get; set; }
		public DateTime Start { get; set; }
		public DateTime Stop { get; set; }

		public override string ToString()
		{
			return Description;
		}
	}
}