using System;
using System.Collections.Generic;

#nullable disable

namespace NEWefficiencyTool.ModelsDB
{
    public partial class TaskEmployeeEfficiency
    {
        public int EfficiencyId { get; set; }
        public string EmployeeId { get; set; }
        public string TaskId { get; set; }
        public double EfficiencyCoef { get; set; }

        public virtual Employee Employee { get; set; }
        public virtual Task Task { get; set; }
    }
}
