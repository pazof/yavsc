using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace Yavsc.Models.Identity
{
  [JsonObject]

  public class GoogleCloudMobileDeclaration : IGCMDeclaration {

    [Required]
    public string GCMRegistrationId { get; set; }

    [Key,Required]
    public string DeviceId { get; set; }

    public string Model { get; set; }
    public string Platform { get; set; }
    public string Version { get; set; }
    public string DeviceOwnerId { get; set; }
    public DateTime DeclarationDate { get; set; }

    /// <summary>
    /// Latest Activity Update
    /// 
    /// Let's says, 
    /// the latest time this device downloaded functional info from server 
    /// activity list, let's say, promoted ones, those thar are at index, and
    /// all others, that are not listed as unsupported ones (not any more, after 
    /// has been annonced as obsolete a decent laps of time).
    /// 
    /// In order to say, is any activity has changed here.
    /// </summary>
    /// <returns></returns>
    public DateTime ? LatestActivityUpdate { get; set; }

    [JsonIgnore,ForeignKey("DeviceOwnerId")]
    public virtual ApplicationUser DeviceOwner { get; set; }


  }

}
