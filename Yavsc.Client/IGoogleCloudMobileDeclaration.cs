namespace Yavsc.Models.Identity
{
    public interface IGoogleCloudMobileDeclaration
    {
        string DeviceId { get; set; }
        IApplicationUser DeviceOwner { get; set; }
        string DeviceOwnerId { get; set; }
        string GCMRegistrationId { get; set; }
        string Model { get; set; }
        string Platform { get; set; }
        string Version { get; set; }
    }
}