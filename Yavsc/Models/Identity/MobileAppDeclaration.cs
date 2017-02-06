using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace Yavsc.Models.Identity
{
   [JsonObject]

  public class GoogleCloudMobileDeclaration {
    
    [Required]
    public string GCMRegistrationId { get; set; }
    
    [Key,Required]
    public string DeviceId { get; set; }

    public string Model { get; set; }
    public string Platform { get; set; }
    public string Version { get; set; }
    public string DeviceOwnerId { get; set; }
    public DateTime DeclarationDate { get; set; }
    [JsonIgnore,ForeignKey("DeviceOwnerId")]
    public virtual ApplicationUser DeviceOwner { get; set; } 

    public DateTime LatestActivityUpdate { get; set; }
  }

}
