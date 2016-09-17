
using System.Collections.Generic;
using System.Threading.Tasks;
using Yavsc.Models.Google.Messaging;
using Yavsc.Models.Messaging;

namespace Yavsc.Services
{
    public interface IGoogleCloudMessageSender
    {
        Task<MessageWithPayloadResponse> NotifyBookQueryAsync(GoogleAuthSettings googlesettings, IEnumerable<string> registrationId, BookQueryEvent ev);
    }
}
