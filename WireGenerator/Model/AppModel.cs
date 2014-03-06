using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WireGenerator.Model
{
    public class AppModel
    {
        public string Name { get; set; }
        public string LoginUser { get; set; }
        public ICollection<MenuItem> Navigation { get; set; }
    }
}
