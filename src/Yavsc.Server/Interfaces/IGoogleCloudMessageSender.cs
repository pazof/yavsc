
using System.Collections.Generic;
using System.Threading.Tasks;
using Yavsc.Interfaces.Workflow;
using Yavsc.Models.Google.Messaging;
using Yavsc.Models.Haircut;
using Yavsc.Models.Messaging;

namespace Yavsc.Services
{
    public interface IYavscMessageSender
    {
        Task<MessageWithPayloadResponse> NotifyBookQueryAsync(
            IEnumerable<string> DeviceIds, 
            RdvQueryEvent ev);

        Task<MessageWithPayloadResponse> NotifyEstimateAsync(
            IEnumerable<string> registrationId, 
            EstimationEvent ev);

        Task<MessageWithPayloadResponse> NotifyHairCutQueryAsync(
            IEnumerable<string> registrationId, 
            HairCutQueryEvent ev);
        Task<MessageWithPayloadResponse> NotifyAsync(
            IEnumerable<string> regids, 
            IEvent yaev);
    }
}
