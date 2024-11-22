using System;
using System.Net;
using System.Threading.Tasks;
using JiraTogglSync.Services;
using Microsoft.Extensions.DependencyInjection;
using Toggl.Api;

namespace JiraTogglSync.CommandLine;

public class Program
{
	private const string DefaultDescriptionTemplate = "{{toggl:id}}: {{toggl:description}}";

	public static async Task Main()
	{
		ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
		var togglApiKey = ConfigurationHelper.GetEncryptedValueFromConfig("toggl-api-key", () => AskFor("Toggl API Key"));
		var jiraWorkItemDescriptionTemplate = ConfigurationHelper.GetValueFromConfig(
			"jira-description-template",
			() => AskFor($"JIRA Description template (default: '{DefaultDescriptionTemplate}')"),
			DefaultDescriptionTemplate,
			v =>
			{
				if (v.Contains("{{toggl:id}}"))
					return true;

				Console.Error.WriteLine("Error: Template must contain placeholder for toggl time entry id: {{toggl:id}}");
				return false;
			});


		var services = new ServiceCollection();
		RegisterJira(services);
		services.AddSingleton<ITimeUtil, TimeUtil>();
		services.AddSingleton(new TogglClientOptions {Key = togglApiKey});
		services.AddSingleton<TogglClient>();
		services.AddSingleton<IExternalWorksheetRepository, TogglRepository>();
		services.AddOptions<TogglRepository.Options>()
			.Configure(o => o.DescriptionTemplate = jiraWorkItemDescriptionTemplate)
			.ValidateDataAnnotations();

		services.AddSingleton<WorksheetSyncService>();
		services.AddOptions<WorksheetSyncService.Options>()
			.Configure(o =>
			{
				o.AgreeToAdd = workLogEntries => ConsoleHelper.Confirm($"***NEW work log entries***\n{string.Join(Environment.NewLine, workLogEntries)}\nADD {workLogEntries.Count} NEW work log entries?");
				o.AgreeToDeleteDuplicates = workLogEntries => ConsoleHelper.Confirm($"***DUPLICATE work log entries***\n{string.Join(Environment.NewLine, workLogEntries)}\nDELETE {workLogEntries.Count} DUPLICATE work log entries?");
				o.AgreeToDeleteOrphaned = workLogEntries => ConsoleHelper.Confirm($"***ORPHANED work log entries***\n{string.Join(Environment.NewLine, workLogEntries)}\nDELETE {workLogEntries.Count} ORPHANED work log entries?");
				o.AgreeToUpdate = workLogEntries => ConsoleHelper.Confirm($"***CHANGED work log entries***\n{string.Join(Environment.NewLine, workLogEntries)}\nUPDATE {workLogEntries.Count} CHANGED work log entries?");
			}).ValidateDataAnnotations();


		await using var sp = services.BuildServiceProvider();

		Console.WriteLine("Toggl: Connected as {0}", await sp.GetRequiredService<IExternalWorksheetRepository>().GetUserInformationAsync());
		Console.WriteLine("JIRA: Connected as {0}", (await sp.GetRequiredService<IJiraRepository>().GetUserInformation()).Email);

		var syncDays = int.Parse(ConfigurationHelper.GetValueFromConfig("syncDays", () => AskFor("Sync how many days")));
		var roundingToMinutes = int.Parse(ConfigurationHelper.GetValueFromConfig("roundingToMinutes", () => AskFor("Round duration to X minutes")));
		var sync = sp.GetRequiredService<WorksheetSyncService>();

		var doPurge = ConfigurationHelper.GetValueFromConfig(
			"jira-purge",
			() => AskFor("Purge JIRA from orphaned and out-of-sync work log items? (y/N)"),
			"n",
			value => value is "y" or "n"
		);
		var jiraKeyPrefixes = ConfigurationHelper.GetValueFromConfig("jira-prefixes", () => AskFor("JIRA Prefixes without the hyphen (comma-separated)"));

		var syncReport = await sync.SynchronizeAsync(
			DateTimeOffset.Now.Date.AddDays(-syncDays),
			DateTimeOffset.Now.Date.AddDays(1),
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

	private static void RegisterJira(IServiceCollection services)
	{
		var jiraInstance = ConfigurationHelper.GetValueFromConfig(
			"jira-instance",
			() => AskFor("JIRA Instance URL"),
			null,
			value =>
			{
				if (Uri.TryCreate(value, UriKind.Absolute, out _))
					return true;

				Console.Error.WriteLine("Error: The JIRA instance must be a valid HTTP address (e.g. https://(your-company).atlassian.net)");
				return false;
			});

		var jiraUsername = ConfigurationHelper.GetValueFromConfig("jira-username", () => AskFor("JIRA Username"));
		var jiraApiToken = ConfigurationHelper.GetEncryptedValueFromConfig("jira-apitoken", () => AskFor("JIRA API Token"));

		services.AddSingleton<IJiraRepository, JiraRestService>();
		services.AddOptions<JiraRestService.Options>()
			.Configure(o =>
			{
				o.Instance = jiraInstance;
				o.Username = jiraUsername;
				o.ApiToken = jiraApiToken;
			}).ValidateDataAnnotations();
	}

	private static string AskFor(string what)
	{
		Console.Write("Please set '{0}': ", what);
		return Console.ReadLine();
	}
}
