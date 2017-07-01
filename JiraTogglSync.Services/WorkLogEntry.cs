using System;
using System.Text.RegularExpressions;
using TechTalk.JiraRestClient;
using Toggl;

namespace JiraTogglSync.Services
{
	public class WorkLogEntry
	{
		public string Id { get; set; }
		public string IssueKey { get; set; }
		public string Description { get; set; }
		public DateTime Start { get; set; }
		public DateTime Stop { get; set; }

		public TimeSpan RoundedDuration { get; set; }
		public string SourceId { get { return GetSourceId(this.Description); } }

		public static string GetSourceId(string input)
		{
			var regex = new Regex(@"\[toggl-id:(?<sourceId>[0-9]+)]");
			var matchResult = regex.Match(input ?? "");
			return matchResult.Success ? matchResult.Groups["sourceId"].Value : null;
		}

		public WorkLogEntry()
		{
		}

		public WorkLogEntry(Worklog worklog, string jiraKey) : this()
		{
			this.Id = worklog.id;
			this.Description = worklog.comment;
			this.Start = worklog.started;
			this.Stop = worklog.started.AddSeconds(worklog.timeSpentSeconds);
			this.IssueKey = jiraKey; //important to capture key, becuase it will be needed to update WorkLog entry in JIRA
		}

		public override string ToString()
		{
			var jiraKey = string.IsNullOrEmpty(this.IssueKey) ? "" : $"[{this.IssueKey}] - ";
			return $"{jiraKey}{Start:d} - {RoundedDuration} - {Description}";
		}

		public void Round(int nbMinutes)
		{
			RoundedDuration = RoundToClosest(Stop - Start, new TimeSpan(0, 0, nbMinutes, 0));
		}

		public bool HasIssueKeyAssigned()
		{
			return !string.IsNullOrEmpty(this.IssueKey);
		}

		private static TimeSpan RoundToClosest(TimeSpan input, TimeSpan precision)
		{
			if (input < TimeSpan.Zero)
			{
				return -RoundToClosest(-input, precision);
			}

			return new TimeSpan(((input.Ticks + precision.Ticks / 2) / precision.Ticks) * precision.Ticks);
		}

		public void Syncronize(WorkLogEntry workLogEntry)
		{
			if (workLogEntry == null)
				return;

			this.Start = workLogEntry.Start;
			this.Stop = workLogEntry.Stop;
			this.Description = workLogEntry.Description;
			this.RoundedDuration = workLogEntry.RoundedDuration;
			//do not update id, because it uniquely identifies the entry
		}

		public bool DifferentFrom(WorkLogEntry workLogEntry)
		{
			return this.Start != workLogEntry.Start
						 || this.Description != workLogEntry.Description
						 || this.RoundedDuration != workLogEntry.RoundedDuration;
		}
	}
}