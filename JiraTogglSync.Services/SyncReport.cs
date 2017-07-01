using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization.Formatters;
using System.Text;

namespace JiraTogglSync.Services
{
	public class SyncReport
	{
		public List<OperationResult> AddedEntries { get; set; }
		public List<OperationResult> UpdatedEntries { get; set; }
		public List<OperationResult> DeletedOrphanedEntries { get; set; }
		public List<OperationResult> DeletedDuplicateEntries { get; set; }
		public List<WorkLogEntry> NoChanges { get; set; }

		public SyncReport()
		{
			this.AddedEntries = new List<OperationResult>();
			this.UpdatedEntries = new List<OperationResult>();
			this.DeletedOrphanedEntries = new List<OperationResult>();
			this.DeletedDuplicateEntries = new List<OperationResult>();
			this.NoChanges = new List<WorkLogEntry>();
		}

		public override string ToString()
		{
			var errorMessages = "";


			var report =
					$@"Syncronization report:
--------------------------------------------
Added new entries:         {this.AddedEntries.Count(e => e.Status == Status.Success)}
Updated existing entries:  {this.UpdatedEntries.Count(e => e.Status == Status.Success)}
Deleted orphaned entries:  {this.DeletedOrphanedEntries.Count(e => e.Status == Status.Success)}
Deleted duplicate entries: {this.DeletedDuplicateEntries.Count(e => e.Status == Status.Success)}
Entries without changes:   {this.NoChanges.Count}
--------------------------------------------
{DisplayErrorMessages()}
";
			return report;
		}

		private string DisplayErrorMessages()
		{
			var allErrors = this.AddedEntries.Where(e => e.Status == Status.Error)
					.Union(this.UpdatedEntries.Where(e => e.Status == Status.Error))
					.Union(this.DeletedDuplicateEntries.Where(e => e.Status == Status.Error))
					.Union(this.DeletedOrphanedEntries.Where(e => e.Status == Status.Error));

			if (allErrors.Any())
			{
				var sb = new StringBuilder();
				var wereErrors = allErrors.Count() == 1 ? "was an ERROR" : "were ERRORS";
				sb.AppendLine($"There {wereErrors} during syncronization process:");
				foreach (var error in allErrors)
				{
					sb.AppendLine(error.OperationArgument.ToString());
					sb.AppendLine("\tError:" + (error.Message ?? "no error message"));
				}

				return sb.ToString();
			}

			return string.Empty;
		}
	}
}