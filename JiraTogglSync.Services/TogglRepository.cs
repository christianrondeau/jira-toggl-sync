using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using Toggl;
using Toggl.QueryObjects;
using Toggl.Services;

namespace JiraTogglSync.Services
{
	public class TogglRepository : IExternalWorksheetRepository
    {
        private readonly string _apiKey;
        private readonly string _descriptionTemplate;

        public TogglRepository(string apiKey, string descriptionTemplate)
        {
            if (apiKey == null) throw new ArgumentNullException("apiKey");
            if (descriptionTemplate == null) throw new ArgumentNullException("descriptionTemplate");

            _apiKey = apiKey;
            _descriptionTemplate = descriptionTemplate;
        }

        public string GetUserInformation()
        {
            var userService = new UserService(_apiKey);
            var currentUser = userService.GetCurrent();
            return string.Format("{0} ({1})", currentUser.FullName, currentUser.Email);
        }

		public IEnumerable<WorkLogEntry> GetEntries(DateTime startDate, DateTime endDate, IEnumerable<string> jiraProjectKeys)
		{
			var timeEntryService = new TimeEntryService(_apiKey);

			var togglTimeEntries = timeEntryService
				.List(new TimeEntryParams
					{
						StartDate = startDate,
						EndDate = endDate
					})
				.Where(w => !string.IsNullOrEmpty(w.Description) && w.Stop != null);

            var jiraWorkLogEntries = togglTimeEntries.Select(t => ToWorkLogEntry(t, _descriptionTemplate, jiraProjectKeys));

            jiraWorkLogEntries = jiraWorkLogEntries.Where(j => j.HasIssueKeyAssigned());

            return jiraWorkLogEntries;
		}

	    private WorkLogEntry ToWorkLogEntry(TimeEntry togglTimeEntry, string descriptionTemplate, IEnumerable<string> jiraProjectKeys)
		{
			return new WorkLogEntry
				{
					Start = DateTime.Parse(togglTimeEntry.Start),
					Stop = DateTime.Parse(togglTimeEntry.Stop),
					Description = CreateDescription(togglTimeEntry, descriptionTemplate),
                    IssueKey = ExtractIssueKey(togglTimeEntry.Description, jiraProjectKeys )
				};
		}

	    public static string ExtractIssueKey(string description, IEnumerable<string> jiraProjectKeys)
	    {
	        if (jiraProjectKeys == null || !jiraProjectKeys.Any())
	            return null;

            var regex = new Regex(string.Format(@"(?<startAnchor>^| |\[)(?<jiraKey>({0})-\d+)", string.Join("|", jiraProjectKeys)));
	        var matchResult = regex.Match(description);
	        return matchResult.Success ? matchResult.Groups["jiraKey"].Value : null;
	    }

	    public static string CreateDescription(TimeEntry timeEntry, string descriptionTemplate)
        {
            var result = descriptionTemplate.Replace("{{toggl:id}}", string.Format("[toggl-id:{0}]", timeEntry.Id))
                                            .Replace("{{toggl:description}}", timeEntry.Description)
                                            .Replace("{{toggl:createdWith}}", timeEntry.CreatedWith)
                                            .Replace("{{toggl:isBillable}}",  timeEntry.IsBillable == null ? "" : timeEntry.IsBillable.ToString())
                                            .Replace("{{toggl:projectId}}", timeEntry.ProjectId == null ? "" : timeEntry.ProjectId.ToString())
                                            .Replace("{{toggl:tagNames}}", string.Join(",", timeEntry.TagNames ?? Enumerable.Empty<string>()))
                                            .Replace("{{toggl:taskId}}", timeEntry.TaskId == null ? "" : timeEntry.TaskId.ToString())
                                            .Replace("{{toggl:updatedOn}}", timeEntry.UpdatedOn == null ?"" : timeEntry.UpdatedOn.ToString());

            return result;

        }
    }
}
