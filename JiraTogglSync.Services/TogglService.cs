using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Toggl;
using Toggl.QueryObjects;
using Toggl.Services;

namespace JiraTogglSync.Services
{
	public class TogglService : IWorksheetSourceService
    {
        private readonly string _apiKey;
        private readonly string _descriptionTemplate;

        public TogglService(string apiKey, string descriptionTemplate)
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

		public IEnumerable<WorkLogEntry> GetEntries(DateTime startDate, DateTime endDate)
		{
			var timeEntryService = new TimeEntryService(_apiKey);

			var hours = timeEntryService
				.List(new TimeEntryParams
					{
						StartDate = startDate,
						EndDate = endDate
					})
				.Where(w => !string.IsNullOrEmpty(w.Description) && w.Stop != null);

			return hours.Select(h => ToWorkLogEntry(h, _descriptionTemplate));
		}

		private WorkLogEntry ToWorkLogEntry(TimeEntry arg, string descriptionTemplate)
		{
			return new WorkLogEntry
				{
					Start = DateTime.Parse(arg.Start),
					Stop = DateTime.Parse(arg.Stop),
					Description = CreateDescription(arg, descriptionTemplate) 
				};
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
