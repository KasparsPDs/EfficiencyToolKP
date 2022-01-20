using System;
using System.Collections.Generic;

#nullable disable

namespace NEWefficiencyTool.ModelsDB
{
    public partial class EmployeeAvgEfficiency
    {
        public int EmplEfficiencyId { get; set; }
        public string EmployeeId { get; set; }
        public double EfficiencyCoef { get; set; }
        public DateTime DateFrom { get; set; }
        public DateTime DateTo { get; set; }

        public virtual Employee Employee { get; set; }
    }
}
