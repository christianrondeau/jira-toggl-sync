using System;
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
			var jiraKeyPrefixes = ConfigurationHelper.GetValueFromConfig("jira-prefixes", () => AskFor("JIRA Prefixes (comma-separated)"));
            var jira = new JiraService(jiraInstance, jiraUsername, jiraPassword);
            Console.WriteLine("JIRA: Connected as {0}", jira.GetUserInformation());

			var sync = new WorksheetSyncService(toggl, jira, jiraKeyPrefixes.Split(','));

	        foreach (var suggestion in sync.GetSuggestions(DateTime.Now.AddDays(-14), DateTime.Now))
	        {
		        Console.WriteLine(suggestion.ToString());
	        }
        }

        private static string AskFor(string what)
        {
            Console.Write("Please enter your {0}: ", what);
            return Console.ReadLine();
        }
    }
}