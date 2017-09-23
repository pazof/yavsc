using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Yavsc.Models.Cratie
{
    public class Option: IBaseTrackedEntity
    {
        public string CodeScrutin { get; set; }
        public string Code { get; set ; }
        public string Description { get; set; }
        public DateTime DateCreated { get ; set ; }
        public string UserCreated { get ; set ; }
        public DateTime DateModified { get ; set ; }
        public string UserModified { get ; set ; }
    }
}