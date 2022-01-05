using System;
using System.Collections.Generic;

#nullable disable

namespace EfficiencyToolKPlocal.Models
{
    public partial class Worklog
    {
        public int WorklogId { get; set; }
        public string EmployeeId { get; set; }
        public string TaskId { get; set; }
        public DateTime WorklogDate { get; set; }
        public TimeSpan? TimeSpent { get; set; }

        public virtual Employee Employee { get; set; }
        public virtual Task Task { get; set; }
    }
}
