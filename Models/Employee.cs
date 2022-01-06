using System;
using System.Collections.Generic;

#nullable disable

namespace EfficiencyToolKPlocal.Models
{
    public partial class Employee
    {
        public Employee()
        {
            TaskAsignees = new HashSet<Task>();
            TaskReporters = new HashSet<Task>();
            Worklogs = new HashSet<Worklog>();
        }

        public string EmployeeId { get; set; }
        public string FirstLastName { get; set; }
        public string Email { get; set; }

        public virtual ICollection<Task> TaskAsignees { get; set; }
        public virtual ICollection<Task> TaskReporters { get; set; }
        public virtual ICollection<Worklog> Worklogs { get; set; }
    }
}
