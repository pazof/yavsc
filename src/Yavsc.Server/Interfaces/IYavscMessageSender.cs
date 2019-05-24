
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
            IEnumerable<string> connectionIds, 
            RdvQueryEvent ev);

        Task<MessageWithPayloadResponse> NotifyEstimateAsync(
            IEnumerable<string> connectionIds, 
            EstimationEvent ev);

        Task<MessageWithPayloadResponse> NotifyHairCutQueryAsync(
            IEnumerable<string> connectionIds, 
            HairCutQueryEvent ev);
        Task<MessageWithPayloadResponse> NotifyAsync(
            IEnumerable<string> connectionIds, 
            IEvent yaev);
    }
}
