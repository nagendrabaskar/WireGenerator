using System;
using System.Collections.Generic;

namespace WireGenerator.Model
{
    public class Section
    {
        public int SectionId { get; set; }
        public string SectionName { get; set; }
        public string Zone { get; set; }
        public ICollection<Field> Fields { get; set; }
    }
}
