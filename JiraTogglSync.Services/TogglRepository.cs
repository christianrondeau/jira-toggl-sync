using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Toggl.Api.DataObjects;
using Toggl.Api.Interfaces;
using Toggl.Api.QueryObjects;

namespace JiraTogglSync.Services;

public interface IExternalWorksheetRepository
{
	Task<WorkLogEntry[]> GetEntriesAsync(DateTime startDate, DateTime endDate, ICollection<string> jiraProjectKeys, int roundMinutes);
	Task<string> GetUserInformationAsync();
}

public class TogglRepository : IExternalWorksheetRepository
{
	public class Options
	{
		[Required]
		public string DescriptionTemplate { get; set; } = null!;
	}

	private readonly ITimeEntryServiceAsync _timeEntryService;
	private readonly IUserServiceAsync _userService;
	private readonly IOptions<Options> _options;
	private readonly ITimeUtil _timeUtil;

	public TogglRepository(
		ITimeEntryServiceAsync timeEntryService,
		IUserServiceAsync userService,
		IOptions<Options> options,
		ITimeUtil timeUtil
	)
	{
		_timeEntryService = timeEntryService;
		_userService = userService;
		_options = options;
		_timeUtil = timeUtil;
	}

	public async Task<string> GetUserInformationAsync()
	{
		var currentUser = await _userService.GetCurrentAsync();
		return $"{currentUser.FullName} ({currentUser.Email})";
	}

	public async Task<WorkLogEntry[]> GetEntriesAsync(
		DateTime startDate,
		DateTime endDate,
		ICollection<string> jiraProjectKeys,
		int roundMinutes
	)
	{
		var timeEntryParams = new TimeEntryParams
		{
			StartDate = startDate,
			EndDate = endDate
		};

		var togglTimeEntries = (await _timeEntryService.GetAllAsync(timeEntryParams))
			.Where(w => !string.IsNullOrEmpty(w.Description) && w.Stop != null);

		return togglTimeEntries
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
		if (togglTimeEntry.Id == null)
			return null;
		var issueKey = ExtractIssueKey(togglTimeEntry.Description, jiraProjectKeys);
		if (issueKey == null)
			return null;

		var startDate = DateTime.Parse(togglTimeEntry.Start);
		var stopDate = DateTime.Parse(togglTimeEntry.Stop);
		var duration = stopDate - startDate;

		var timeSpentInMinutes = (int) _timeUtil.RoundToClosest(
			duration,
			TimeSpan.FromMinutes(roundMinutes)
		).TotalMinutes;

		return new WorkLogEntry(
			issueKey,
			togglTimeEntry.Id.Value.ToString(),
			_timeUtil.RoundDateTimeToCloses(startDate, TimeSpan.FromMinutes(roundMinutes)),
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
			.Replace("{{toggl:createdWith}}", timeEntry.CreatedWith)
			.Replace("{{toggl:isBillable}}", timeEntry.IsBillable == null ? "" : timeEntry.IsBillable.ToString())
			.Replace("{{toggl:projectId}}", timeEntry.ProjectId == null ? "" : timeEntry.ProjectId.ToString())
			.Replace("{{toggl:tagNames}}", string.Join(",", timeEntry.TagNames ?? Enumerable.Empty<string>()))
			.Replace("{{toggl:taskId}}", timeEntry.TaskId == null ? "" : timeEntry.TaskId.ToString())
			.Replace("{{toggl:updatedOn}}", timeEntry.UpdatedOn == null ? "" : timeEntry.UpdatedOn);

		return result;
	}
}