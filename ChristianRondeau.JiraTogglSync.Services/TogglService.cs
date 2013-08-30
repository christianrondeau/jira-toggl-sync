using System;
using System.Collections.Generic;
using System.Linq;
using Toggl;
using Toggl.QueryObjects;
using Toggl.Services;

namespace ChristianRondeau.JiraTogglSync.Services
{
	public class TogglService : IWorksheetSourceService
    {
        private readonly string _apiKey;

        public TogglService(string apiKey)
        {
            _apiKey = apiKey;
        }

        public string GetUserInformation()
        {
            var userService = new UserService(_apiKey);
            var currentUser = userService.GetCurrent();
            return string.Format("{0} ({1})", currentUser.FullName, currentUser.Email);
        }

		public IEnumerable<WorkLogEntry> GetEntries(DateTime startDate, DateTime endDate)
		{
			var timeSrv = new TimeEntryService(_apiKey);
			var prams = new TimeEntryParams
				{
					StartDate = startDate,
					EndDate = endDate
				};

			var hours = timeSrv.List(prams)
			                   .Where(w => !string.IsNullOrEmpty(w.Description));

			return hours.Select(ToWorkLogEntry);
		}

		private static WorkLogEntry ToWorkLogEntry(TimeEntry arg)
		{
			return new WorkLogEntry
				{
					Description = arg.Description
				};
		}
    }
}
