using System.ComponentModel.DataAnnotations;

namespace Yavsc.Models.Identity
{
    public class ReservedUserName : IWatchedUserName {

        [Key]
        [StringLength(1024)]
        public string Name { get; set;}

    }
}