using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Yavsc.Models;

public class GoogleCloudMobileDeclaration {
    [Key]

    public string RegistrationId { get; set; }
    
    public string DeviceOwnerId { get; set; }
    
    [ForeignKeyAttribute("DeviceOwnerId")]
    public virtual ApplicationUser DeviceOwner { get; set; }

}
