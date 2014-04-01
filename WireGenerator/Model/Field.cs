using System;
using System.Collections.Generic;

namespace WireGenerator.Model
{
    public class Field
    {
        public string FieldName { get; set; }
        public int Type { get; set; }
        public int DataType { get; set; }
        public int Index { get; set; }
        public ICollection<FieldOption> FieldOptions { get; set; }
    }
}
