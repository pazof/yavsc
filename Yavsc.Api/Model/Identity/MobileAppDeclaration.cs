using System.ComponentModel.DataAnnotations;

public class GoogleCloudMobileDeclaration {

    public GoogleCloudMobileDeclaration() {

    }

    public string GCMRegistrationId { get; set; }
    
    public string DeviceOwnerId { get; set; }

    [Key]
    public string DeviceId { get; set; }
    
    public string Model { get; set; }
    public string Platform { get; set; }
    public string Version { get; set; }
    /*
    [ForeignKeyAttribute("DeviceOwnerId")]
    public virtual ApplicationUser DeviceOwner { get; set; } */
}
