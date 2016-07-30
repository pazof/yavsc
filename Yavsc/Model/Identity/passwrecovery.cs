using System;

namespace Yavsc.Models
{
    public partial class passwrecovery
    {
        public string pkid { get; set; }
        public DateTime creation { get; set; }
        public string one_time_pass { get; set; }
    }
}
