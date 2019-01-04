using System.ComponentModel.DataAnnotations;

namespace Yavsc.Models.Identity
{
    public class BlackListedUserName : IWatchedUserName {

        [Key]
        [StringLength(1024)]
        public string Name { get; set;}

    }
}