using System;
using System.ComponentModel.DataAnnotations;

namespace Yavsc.Models.Cratie
{
    public class Scrutin : ITrackedEntity
    {
        [Key]
        public string Code { get; set ; }
        public string Description { get ; set ; }
        public DateTime DateCreated { get; set; }
        public string UserCreated { get; set; }
        public DateTime DateModified { get; set; }
        public string UserModified { get; set; }
    }
}
