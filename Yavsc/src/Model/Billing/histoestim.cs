using System;

namespace Yavsc.Models
{
    public partial class histoestim
    {
        public long _id { get; set; }
        public string applicationname { get; set; }
        public DateTime datechange { get; set; }
        public long estid { get; set; }
        public int? status { get; set; }
        public string username { get; set; }
    }
}
