using System;
using System.Linq;
using ChristianRondeau.JiraTogglSync.Services;

namespace ChristianRondeau.JiraTogglSync.CommandLine
{
	class Program
	{
		static void Main(string[] args)
		{
			var togglApiKey = ConfigurationHelper.GetEncryptedValueFromConfig("toggl-api-key", () => AskFor("Toggl API Key"));
			var toggl = new TogglService(togglApiKey);
			Console.WriteLine("Toggl: Connected as {0}", toggl.GetUserInformation());

			var jiraInstance = ConfigurationHelper.GetValueFromConfig("jira-instance", () => AskFor("JIRA Instance"));
			var jiraUsername = ConfigurationHelper.GetValueFromConfig("jira-username", () => AskFor("JIRA Username"));
			var jiraPassword = ConfigurationHelper.GetEncryptedValueFromConfig("jira-password", () => AskFor("JIRA Password"));
			var jiraKeyPrefixes = ConfigurationHelper.GetValueFromConfig("jira-prefixes", () => AskFor("JIRA Prefixes without the hyphen (comma-separated)"));
			var jira = new JiraService(jiraInstance, jiraUsername, jiraPassword);
			Console.WriteLine("JIRA: Connected as {0}", jira.GetUserInformation());

			var syncDays = int.Parse(ConfigurationHelper.GetValueFromConfig("syncDays", () => AskFor("Sync how many days")));

			var sync = new WorksheetSyncService(toggl, jira, jiraKeyPrefixes.Split(','));

			var suggestions = sync.GetSuggestions(DateTime.Now.Date.AddDays(-syncDays), DateTime.Now).ToList();
			suggestions.ForEach(x => x.WorkLog.ForEach(y => y.Round()));

			foreach (var issue in suggestions)
			{
				var issueTitle = issue.ToString();
				Console.WriteLine(issueTitle);
				Console.WriteLine(new String('=', issueTitle.Length));

				foreach (var entry in issue.WorkLog.Where(entry => entry.RoundedDuration.Ticks > 0))
				{
					Console.Write(entry + " (y/n)");
					if (Console.ReadKey(true).KeyChar == 'y')
					{
						sync.AddWorkLog(entry);
						Console.Write(" Done");
					}
					Console.WriteLine();
				}

				Console.WriteLine();
			}
		}

		private static string AskFor(string what)
		{
			Console.Write("Please enter your {0}: ", what);
			return Console.ReadLine();
		}
	}
}