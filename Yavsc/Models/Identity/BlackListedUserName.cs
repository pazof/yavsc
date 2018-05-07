using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace Yavsc.Models.Identity
{
    public class BlackListedUserName : IWatchedUserName {

        [Key]
        [StringLength(1024)]
        public string Name { get; set;}

    }
}