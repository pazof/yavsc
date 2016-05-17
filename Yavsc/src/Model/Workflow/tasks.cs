using System;

namespace Yavsc.Models
{
    public partial class tasks
    {
        public long id { get; set; }
        public DateTime endd { get; set; }
        public string name { get; set; }
        public long prid { get; set; }
        public DateTime start { get; set; }
        public string tdesc { get; set; }
    }
}
