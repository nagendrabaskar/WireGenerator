using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WireGenerator.Model
{
    public class Section
    {
        public string SectionName { get; set; }
        public string Zone { get; set; }
        public ICollection<Field> Fields { get; set; }
    }
}
