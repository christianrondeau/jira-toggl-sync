namespace ChristianRondeau.JiraTogglSync.Services
{
	public class WorkLogEntry
	{
		public string Description { get; set; }

		public override string ToString()
		{
			return Description;
		}
	}
}