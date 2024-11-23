using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Toggl.Api;
using Toggl.Api.Models;

namespace JiraTogglSync.Services;

public interface IExternalWorksheetRepository
{
	Task<WorkLogEntry[]> GetEntriesAsync(DateTimeOffset startDate, DateTimeOffset endDate, ICollection<string> jiraProjectKeys, int roundMinutes);
	Task<string> GetUserInformationAsync();
}

public class TogglRepository : IExternalWorksheetRepository
{
	public class Options
	{
		[Required]
		public string DescriptionTemplate { get; set; } = null!;
	}

	private readonly TogglClient _togglClient;
	private readonly IOptions<Options> _options;
	private readonly ITimeUtil _timeUtil;

	public TogglRepository(
		TogglClient togglClient,
		IOptions<Options> options,
		ITimeUtil timeUtil
	)
	{
		_togglClient = togglClient;
		_options = options;
		_timeUtil = timeUtil;
	}

	public async Task<string> GetUserInformationAsync()
	{
		var currentUser = await _togglClient.Me.GetAsync(false, CancellationToken.None);
		return $"{currentUser.FullName} ({currentUser.Email})";
	}

	public async Task<WorkLogEntry[]> GetEntriesAsync(
		DateTimeOffset startDate,
		DateTimeOffset endDate,
		ICollection<string> jiraProjectKeys,
		int roundMinutes
	)
	{
		var togglTimeEntries = (await _togglClient.TimeEntries.GetAsync(true, false, null, null, startDate, endDate, CancellationToken.None))
			.Where(w => !string.IsNullOrEmpty(w.Description) && w.Stop != null);

		return togglTimeEntries
			.Where(x => x.Start > startDate)
			.Select(t => ToWorkLogEntry(t, _options.Value.DescriptionTemplate, jiraProjectKeys, roundMinutes))
			.WhereNotNull()
			.ToArray();
	}

	private WorkLogEntry? ToWorkLogEntry(
		TimeEntry togglTimeEntry,
		string descriptionTemplate,
		ICollection<string> jiraProjectKeys,
		int roundMinutes
	)
	{
		if (togglTimeEntry.Start == null)
			return null;
		if (togglTimeEntry.Stop == null)
			return null;
		var issueKey = ExtractIssueKey(togglTimeEntry.Description, jiraProjectKeys);
		if (issueKey == null)
			return null;

		var startDate = togglTimeEntry.Start;
		var stopDate = togglTimeEntry.Stop;
		var duration = stopDate - startDate;

		var timeSpentInMinutes = (int) _timeUtil.RoundToClosest(
			duration.Value,
			TimeSpan.FromMinutes(roundMinutes)
		).TotalMinutes;

		return new WorkLogEntry(
			issueKey,
			togglTimeEntry.Id.ToString(),
			_timeUtil.RoundDateTimeToCloses(startDate.Value, TimeSpan.FromMinutes(roundMinutes)),
			timeSpentInMinutes,
			CreateDescription(togglTimeEntry, descriptionTemplate)
		);
	}

	public static string? ExtractIssueKey(string? description, ICollection<string> jiraProjectKeys)
	{
		if (description == null)
			return null;
		if (!jiraProjectKeys.Any())
			return null;

		var regex = new Regex($@"(?<startAnchor>^| |\[)(?<jiraKey>({string.Join("|", jiraProjectKeys)})-\d+)");
		var matchResult = regex.Match(description);
		return matchResult.Success ? matchResult.Groups["jiraKey"].Value : null;
	}

	private static string CreateDescription(TimeEntry timeEntry, string descriptionTemplate)
	{
		var result = descriptionTemplate.Replace("{{toggl:id}}", $"[toggl-id:{timeEntry.Id}]")
			.Replace("{{toggl:description}}", timeEntry.Description)
			.Replace("{{toggl:isBillable}}", timeEntry.Billable.ToString())
			.Replace("{{toggl:projectId}}", timeEntry.ProjectId == null ? "" : timeEntry.ProjectId.ToString())
			.Replace("{{toggl:tagNames}}", string.Join(",", timeEntry.Tags ?? Enumerable.Empty<string>()))
			.Replace("{{toggl:taskId}}", timeEntry.TaskId == null ? "" : timeEntry.TaskId.ToString());

		return result;
	}
}