using System.ComponentModel.DataAnnotations;
using Yavsc.Attributes.Validation;
namespace Yavsc.Models.FileSystem
{

    public class MoveFileQuery
    {
        [ValidRemoteUserFilePath]
        [StringLength(512)]
        [Required]
        public required string Id { get; set; }

        [StringLength(512)]
        [ValidRemoteUserFilePath]
        [Required]
        public required string To { get; set; }
    }

}
