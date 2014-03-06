using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WireGenerator.Model
{
    public class MenuItem
    {
        public string Name { get; set; }
        public string LinkToUrl { get; set; }
        public ICollection<MenuSubItem> MenuSubItems  { get; set; }
    }
}
