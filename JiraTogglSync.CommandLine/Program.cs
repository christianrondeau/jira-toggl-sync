using System;
using System.Linq;
using JiraTogglSync.Services;
using System.Collections.Generic;

namespace JiraTogglSync.CommandLine
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
			var jiraWorklogStrategy = ConfigurationHelper.GetValueFromConfig("jira-worklogStrategy", () => AskFor("JIRA Worklog strategy (AutoAdjustRemainingEstimate, RetainRemainingEstimate (default))"));
			var jira = new JiraRestService(jiraInstance, jiraUsername, jiraPassword, jiraWorklogStrategy);
			Console.WriteLine("JIRA: Connected as {0}", jira.GetUserInformation());

			var syncDays = int.Parse(ConfigurationHelper.GetValueFromConfig("syncDays", () => AskFor("Sync how many days")));
			var roundingToMinutes = int.Parse(ConfigurationHelper.GetValueFromConfig("roundingToMinutes", () => AskFor("Round duration to X minutes")));

			var sync = new WorksheetSyncService(toggl, jira, jiraKeyPrefixes.Split(','));

			var suggestions = sync.GetSuggestions(DateTime.Now.Date.AddDays(-syncDays), DateTime.Now.Date.AddDays(1)).ToList();
			suggestions.ForEach(x => x.WorkLog.ForEach(y => y.Round(roundingToMinutes)));

			var entriesToSync = new List<WorkLogEntry>();
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
						entriesToSync.Add(entry);
						Console.Write(" Done");
					}
					Console.WriteLine();
				}

				Console.WriteLine();
			}
			if(!entriesToSync.Any()) return;
			
			Console.WriteLine();
			Console.Write("Send to Jira? (y/n)");
			if (Console.ReadKey(true).KeyChar == 'y')
			{
				foreach (var entry in entriesToSync)
					sync.AddWorkLog(entry);
			}
		}

		private static string AskFor(string what)
		{
			Console.Write("Please enter your {0}: ", what);
			return Console.ReadLine();
		}
	}
}