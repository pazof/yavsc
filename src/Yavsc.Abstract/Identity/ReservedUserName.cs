using System.ComponentModel.DataAnnotations;
using Yavsc.Attributes.Validation;

namespace Yavsc.Models.Identity
{
    public class ReservedUserName : IWatchedUserName {

        [Key]
        [YaStringLength(1024)]
        public string Name { get; set;}

    }
}