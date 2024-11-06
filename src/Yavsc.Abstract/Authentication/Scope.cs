
using System.ComponentModel.DataAnnotations;

namespace Yavsc.Models.Auth {
    public class Scope {
        
        
    [Key][Required]

    public string Id { get; set; }

    [MaxLength(1024)][Required]

    public string Description { get; set; }

    }
}
