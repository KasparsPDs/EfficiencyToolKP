using System;
using System.Collections.Generic;

#nullable disable

namespace NEWefficiencyTool.ModelsDB
{
    public partial class Project
    {
        public Project()
        {
            Tasks = new HashSet<Task>();
        }

        public string ProjectKey { get; set; }
        public string ProjectName { get; set; }

        public virtual ICollection<Task> Tasks { get; set; }
    }
}
