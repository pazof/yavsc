
using System.Threading.Tasks;
using Yavsc.Models.Google.Messaging;
using Yavsc.Models.Messaging;

namespace Yavsc.Services
{
    public interface IGoogleCloudMessageSender
    {
        Task<MessageWithPayloadResponse> NotifyAsync(GoogleAuthSettings googlesettings, string registrationId, YaEvent ev);
    }
}
