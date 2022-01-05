using System;
using System.Collections.Generic;

#nullable disable

namespace EfficiencyToolKPlocal.Models
{
    public partial class Task
    {
        public Task()
        {
            Worklogs = new HashSet<Worklog>();
        }

        public string TaskId { get; set; }
        public string TaskName { get; set; }
        public string ReporterId { get; set; }
        public string AsigneeId { get; set; }
        public string ProjectKey { get; set; }
        public TimeSpan? EstimatedTime { get; set; }
        public TimeSpan? ElapsedTime { get; set; }
        public bool? Done { get; set; }
        public DateTime? DoneDate { get; set; }

        public virtual Employee Asignee { get; set; }
        public virtual Project ProjectKeyNavigation { get; set; }
        public virtual Employee Reporter { get; set; }
        public virtual ICollection<Worklog> Worklogs { get; set; }
    }
}
