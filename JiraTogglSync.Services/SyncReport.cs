using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JiraTogglSync.Services;

public class SyncReport
{
	public List<OperationResult> AddedEntries { get; set; } = new();
	public List<OperationResult> UpdatedEntries { get; set; } = new();
	public List<OperationResult> DeletedOrphanedEntries { get; set; } = new();
	public List<OperationResult> DeletedDuplicateEntries { get; set; } = new();
	public List<WorkLogEntry> NoChanges { get; set; } = new();

	public override string ToString()
	{
		var report =
			$@"Syncronization report:
--------------------------------------------
Added new entries:         {AddedEntries.Count(e => e.Status == OperationResult.OperationStatus.Success)}
Updated existing entries:  {UpdatedEntries.Count(e => e.Status == OperationResult.OperationStatus.Success)}
Deleted orphaned entries:  {DeletedOrphanedEntries.Count(e => e.Status == OperationResult.OperationStatus.Success)}
Deleted duplicate entries: {DeletedDuplicateEntries.Count(e => e.Status == OperationResult.OperationStatus.Success)}
Entries without changes:   {NoChanges.Count}
--------------------------------------------
{DisplayErrorMessages()}
";
		return report;
	}

	private string DisplayErrorMessages()
	{
		var allErrors = AddedEntries.Where(e => e.Status == OperationResult.OperationStatus.Error)
			.Union(UpdatedEntries.Where(e => e.Status == OperationResult.OperationStatus.Error))
			.Union(DeletedDuplicateEntries.Where(e => e.Status == OperationResult.OperationStatus.Error))
			.Union(DeletedOrphanedEntries.Where(e => e.Status == OperationResult.OperationStatus.Error))
			.ToList();

		if (allErrors.Any())
		{
			var sb = new StringBuilder();
			var wereErrors = allErrors.Count == 1 ? "was an ERROR" : "were ERRORS";
			sb.AppendLine($"There {wereErrors} during synchronization process:");
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