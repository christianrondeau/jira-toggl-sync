using System;
using System.Linq;
using JiraTogglSync.Services;
using System.Collections.Generic;
using Toggl.Services;

namespace JiraTogglSync.CommandLine
{
	public class Program
	{
		private const string DefaultDescriptionTemplate = "{{toggl:id}}: {{toggl:description}}";

		public static void Main(string[] args)
		{
			var togglApiKey = ConfigurationHelper.GetEncryptedValueFromConfig("toggl-api-key", () => AskFor("Toggl API Key"));
			var jiraWorkItemDescriptionTemplate = ConfigurationHelper.GetValueFromConfig(
				"jira-decription-template",
				() => AskFor($"JIRA Description template (default: '{DefaultDescriptionTemplate}')"),
				DefaultDescriptionTemplate,
				v =>
				{
					if (v.Contains("{{toggl:id}}"))
						return true;

					Console.Error.WriteLine("Error: Template must contain placeholder for toggl time entry id: {{toggl:id}}");
					return false;
				});
		    
            //if number of dependencies grows, we will need to use a container
			var toggl = new TogglRepository( 
                new TimeEntryService(togglApiKey), 
                new UserService(togglApiKey), 
                jiraWorkItemDescriptionTemplate);

			Console.WriteLine("Toggl: Connected as {0}", toggl.GetUserInformation());

			var jiraInstance = ConfigurationHelper.GetValueFromConfig(
				"jira-instance",
				() => AskFor("JIRA Instance"),
				null,
				value =>
				{
					Uri url;
					if (Uri.TryCreate(value, UriKind.Absolute, out url))
						return true;

					Console.Error.WriteLine("Error: The JIRA instance must be a valid HTTP address (e.g. https://(yourcompany).atlassian.net)");
					return false;
				});

			var jiraUsername = ConfigurationHelper.GetValueFromConfig("jira-username", () => AskFor("JIRA Username"));
			var jiraPassword = ConfigurationHelper.GetEncryptedValueFromConfig("jira-password", () => AskFor("JIRA Password"));
			var jiraKeyPrefixes = ConfigurationHelper.GetValueFromConfig("jira-prefixes", () => AskFor("JIRA Prefixes without the hyphen (comma-separated)"));
            var doPurge = ConfigurationHelper.GetValueFromConfig(
                "jira-purge",
                () => AskFor("Purge JIRA from orphaned and out-of-sync work log items? (y/n)"),
                "n",
                value => value == "y" || value == "n"
                );
            var jira = new JiraRestService(jiraInstance, jiraUsername, jiraPassword);
			Console.WriteLine("JIRA: Connected as {0}", jira.GetUserInformation());

			var syncDays = int.Parse(ConfigurationHelper.GetValueFromConfig("syncDays", () => AskFor("Sync how many days")));
			var roundingToMinutes = int.Parse(ConfigurationHelper.GetValueFromConfig("roundingToMinutes", () => AskFor("Round duration to X minutes")));

			var sync = new WorksheetSyncService(toggl, jira);
		    sync.AgreeToAdd = workLogEntries => ConsoleHelper.Confirm($"***NEW work log entries***\n{string.Join(Environment.NewLine,workLogEntries)}\nADD {workLogEntries.Count()} NEW work log entries?");
		    sync.AgreeToDeleteDuplicates = workLogEntries => ConsoleHelper.Confirm($"***DUPLICATE work log entries***\n{string.Join(Environment.NewLine, workLogEntries)}\nDELETE {workLogEntries.Count()} DUPLICATE work log entries?");
		    sync.AgreeToDeleteOrphaned = workLogEntries => ConsoleHelper.Confirm($"***ORPHANED work log entries***\n{string.Join(Environment.NewLine, workLogEntries)}\nDELETE {workLogEntries.Count()} ORPHANED work log entries?");
		    sync.AgreeToUpdate = workLogEntries => ConsoleHelper.Confirm($"***CHANGED work log entries***\n{string.Join(Environment.NewLine, workLogEntries)}\nUPDATE {workLogEntries.Count()} CHANGED work log entries?");

            var syncReport = sync.Syncronize(
                DateTime.Now.Date.AddDays(-syncDays), 
                DateTime.Now.Date.AddDays(1), 
                jiraKeyPrefixes.Split(','),
		        doPurge == "y",
                roundingToMinutes
                );

            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine(syncReport.ToString());

            Console.WriteLine("Press any key to exit.");
            Console.ReadKey();
		}

		private static string AskFor(string what)
		{
			Console.Write("Please enter your {0}: ", what);
			return Console.ReadLine();
		}
	}
}