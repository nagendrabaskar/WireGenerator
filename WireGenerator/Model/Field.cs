using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WireGenerator.Model
{
    public class Field
    {
        public string FieldName { get; set; }
        public int Type { get; set; }
        public ICollection<FieldOption> FieldOptions { get; set; }
    }
}
