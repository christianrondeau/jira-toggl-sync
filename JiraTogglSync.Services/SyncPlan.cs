using System.Collections.Generic;
// ReSharper disable PropertyCanBeMadeInitOnly.Global

namespace JiraTogglSync.Services;

public class SyncPlan
{
	public List<WorkLogEntry> NoChanges { get; set; }
	public List<WorkLogEntry> ToDeleteDuplicates { get; set; }
	public List<WorkLogEntry> ToDeleteOrphaned { get; set; }
	public List<WorkLogEntry> ToUpdate { get; set; }
	public List<WorkLogEntry> ToAdd { get; set; }

	public SyncPlan()
	{
		NoChanges = new List<WorkLogEntry>();
		ToDeleteDuplicates = new List<WorkLogEntry>();
		ToDeleteOrphaned = new List<WorkLogEntry>();
		ToUpdate = new List<WorkLogEntry>();
		ToAdd = new List<WorkLogEntry>();
	}
}