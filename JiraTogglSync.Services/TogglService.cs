﻿using System;
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
        private readonly string _decriptionTemplate;

        public TogglService(string apiKey, string decriptionTemplate)
        {
            if (apiKey == null) throw new ArgumentNullException(nameof(apiKey));
            if (decriptionTemplate == null) throw new ArgumentNullException(nameof(decriptionTemplate));

            _apiKey = apiKey;
            _decriptionTemplate = decriptionTemplate;
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

			return hours.Select(h => ToWorkLogEntry(h, _decriptionTemplate));
		}

		private WorkLogEntry ToWorkLogEntry(TimeEntry arg, string desrciptionTemplate)
		{
			return new WorkLogEntry
				{
					Start = DateTime.Parse(arg.Start),
					Stop = DateTime.Parse(arg.Stop),
					Description = CreateDescription(arg, desrciptionTemplate) 
				};
		}

        public static string CreateDescription(TimeEntry timeEntry, string descriptionTemplate)
        {
            var result = descriptionTemplate.Replace("{{toggl:id}}", $"[toggl-id:{timeEntry.Id}]")
                                            .Replace("{{toggl:description}}", timeEntry.Description)
                                            .Replace("{{toggl:createdWith}}", timeEntry.CreatedWith)
                                            .Replace("{{toggl:isBillable}}", timeEntry.IsBillable.ToString())
                                            .Replace("{{toggl:projectId}}", timeEntry.ProjectId.ToString())
                                            .Replace("{{toggl:tagNames}}", string.Join(",", timeEntry.TagNames ?? Enumerable.Empty<string>()))
                                            .Replace("{{toggl:taskId}}", timeEntry.TaskId.ToString())
                                            .Replace("{{toggl:updatedOn}}", timeEntry.UpdatedOn.ToString());

            return result;

        }
    }
}
