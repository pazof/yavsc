using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZicMoove.Attributes
{
    class DisplayAttribute : Attribute
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public DisplayAttribute()
        {

        }
        public DisplayAttribute(string name)
        {
            Name = name;
        }
    }
}
