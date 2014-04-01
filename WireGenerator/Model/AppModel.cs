using System;
using System.Collections.Generic;

namespace WireGenerator.Model
{
    public class AppModel
    {
        public string Name { get; set; }
        public string LoginUser { get; set; }
        public ICollection<MenuItem> Navigation { get; set; }
        public ICollection<Entity> Entities { get; set; }
    }
}
