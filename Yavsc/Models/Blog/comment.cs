using System;

namespace Yavsc.Models
{
    public partial class comment
    {
        public long _id { get; set; }
        public string applicationname { get; set; }
        public string bcontent { get; set; }
        public DateTime modified { get; set; }
        public DateTime posted { get; set; }
        public long? postid { get; set; }
        public string username { get; set; }
        public bool visible { get; set; }
    }
}
