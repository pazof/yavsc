
namespace Yavsc.Interfaces
{
    public interface IGCMDeclaration
    {
        string DeviceId { get; set; }
        string GCMRegistrationId { get; set; }
        string Model { get; set; }
        string Platform { get; set; }
        string Version { get; set; }

    }

    public interface IGoogleCloudMobileDeclaration: IGCMDeclaration
    {
        IApplicationUser DeviceOwner { get; set; }
        string DeviceOwnerId { get; set; }
    }
}
