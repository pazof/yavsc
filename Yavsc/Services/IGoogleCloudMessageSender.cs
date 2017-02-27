
using System.Collections.Generic;
using System.Threading.Tasks;
using Yavsc.Models.Google.Messaging;
using Yavsc.Models.Haircut;
using Yavsc.Models.Messaging;

namespace Yavsc.Services
{
    public interface IGoogleCloudMessageSender
    {
        Task<MessageWithPayloadResponse> NotifyBookQueryAsync(
            GoogleAuthSettings googlesettings, 
            IEnumerable<string> registrationId, 
            RdvQueryEvent ev);

        Task<MessageWithPayloadResponse> NotifyEstimateAsync(
            GoogleAuthSettings googlesettings, 
            IEnumerable<string> registrationId, 
            EstimationEvent ev);

        Task<MessageWithPayloadResponse> NotifyHairCutQueryAsync(
            GoogleAuthSettings googlesettings, 
            IEnumerable<string> registrationId, 
            HairCutQueryEvent ev);


    }
}
