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
        public IEnumerable<WorkLogEntry> NoChanges { get; set; }
        public IEnumerable<WorkLogEntry> ToDelete { get; set; }
        public IEnumerable<WorkLogEntry> ToUpdate { get; set; }
        public IEnumerable<WorkLogEntry> ToAdd { get; set; }

        public SyncPlan()
        {
            this.NoChanges = new List<WorkLogEntry>();
            this.ToDelete = new List<WorkLogEntry>();
            this.ToUpdate = new List<WorkLogEntry>();
            this.ToAdd = new List<WorkLogEntry>();
        }
    }
}
