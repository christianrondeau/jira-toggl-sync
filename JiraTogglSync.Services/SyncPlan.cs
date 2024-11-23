using System.Collections.Generic;

namespace JiraTogglSync.Services;

public class SyncPlan
{
	public List<WorkLogEntry> NoChanges { get; init; } = [];
	public List<WorkLogEntry> ToDeleteDuplicates { get; init; } = [];
	public List<WorkLogEntry> ToDeleteOrphaned { get; init; } = [];
	public List<WorkLogEntry> ToUpdate { get; init; } = [];
	public List<WorkLogEntry> ToAdd { get; init; } = [];
}