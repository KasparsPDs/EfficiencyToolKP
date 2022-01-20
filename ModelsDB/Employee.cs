using System;
using System.Collections.Generic;

#nullable disable

namespace NEWefficiencyTool.ModelsDB
{
    public partial class Employee
    {
        public Employee()
        {
            EmployeeAvgEfficiencies = new HashSet<EmployeeAvgEfficiency>();
            TaskAsignees = new HashSet<Task>();
            TaskEmployeeEfficiencies = new HashSet<TaskEmployeeEfficiency>();
            TaskReporters = new HashSet<Task>();
            Worklogs = new HashSet<Worklog>();
        }

        public string EmployeeId { get; set; }
        public string FirstLastName { get; set; }
        public string Email { get; set; }

        public virtual ICollection<EmployeeAvgEfficiency> EmployeeAvgEfficiencies { get; set; }
        public virtual ICollection<Task> TaskAsignees { get; set; }
        public virtual ICollection<TaskEmployeeEfficiency> TaskEmployeeEfficiencies { get; set; }
        public virtual ICollection<Task> TaskReporters { get; set; }
        public virtual ICollection<Worklog> Worklogs { get; set; }
    }
}
