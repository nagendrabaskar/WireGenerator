using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WireGenerator.Model
{
    public class Entity
    {
        public string Name { get; set; }
        public string Title { get; set; }
        public bool HasSearch { get; set; }
        public bool ListScreenHasSelector { get; set; }
        public ICollection<Field> ListScreenFields { get; set; }
        public ICollection<Section> AddScreenSections { get; set; }
        public ICollection<Action> Actions { get; set; }
        public Workflow Workflow { get; set; }
    }
}
