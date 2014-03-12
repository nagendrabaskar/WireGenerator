using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WireGenerator.Model
{
    public class Workflow
    {
        public string Name { get; set; }
        public ICollection<WorkflowPhase> Phases { get; set; }
    }
}
