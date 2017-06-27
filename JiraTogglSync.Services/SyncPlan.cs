using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JiraTogglSync.Services
{
    public class SyncPlan
    {
        public List<WorkLogEntry> NoChanges { get; set; }
        public List<WorkLogEntry> ToDeleteDuplicates { get; set; }
        public List<WorkLogEntry> ToDeleteOrphaned { get; set; }
        public List<WorkLogEntry> ToUpdate { get; set; }
        public List<WorkLogEntry> ToAdd { get; set; }

        public SyncPlan()
        {
            this.NoChanges = new List<WorkLogEntry>();
            this.ToDeleteDuplicates = new List<WorkLogEntry>();
            this.ToDeleteOrphaned = new List<WorkLogEntry>();
            this.ToUpdate = new List<WorkLogEntry>();
            this.ToAdd = new List<WorkLogEntry>();
        }

    }
}
