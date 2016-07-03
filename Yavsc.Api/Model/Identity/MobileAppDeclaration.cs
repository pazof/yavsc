using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Yavsc.Models;

public class GoogleCloudMobileDeclaration {

    public string GCMRegistrationId { get; set; }
    
    public string DeviceOwnerId { get; set; }

    [Key]
    public string DeviceId { get; set; }
    
    public string Model { get; set; }
    public string Platform { get; set; }
    public string Version { get; set; }
    
    [ForeignKeyAttribute("DeviceOwnerId")]
    public virtual ApplicationUser DeviceOwner { get; set; }
}
