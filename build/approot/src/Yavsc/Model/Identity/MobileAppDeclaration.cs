using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Yavsc.Models;

public class GoogleCloudMobileDeclaration {
    [Key]

    public string RegistrationId { get; set; }
    
    public string DeviceOwnerId { get; set; }
    
    public string Name { get; set; }
    
    [ForeignKeyAttribute("DeviceOwnerId")]
    public virtual ApplicationUser DeviceOwner { get; set; }
    
    // override object.Equals
    public override bool Equals (object obj)
    {
        if (obj == null || GetType() != obj.GetType())
        {
            return false;
        }
        
        var other = obj as GoogleCloudMobileDeclaration;
        return RegistrationId == other.RegistrationId
        && Name == other.Name;
    }
    
    // override object.GetHashCode
    public override int GetHashCode()
    {
        return (RegistrationId+Name).GetHashCode();
    }

}
