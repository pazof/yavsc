using System.Linq;
using Microsoft.Data.Entity;
using Microsoft.Extensions.Localization;

namespace Yavsc.Models.Messaging
{
    using Interfaces.Workflow;
    using Billing;
    using Yavsc.Helpers;

    public class EstimationEvent: IEvent
    {
        public EstimationEvent(ApplicationDbContext context, Estimate estimate, IStringLocalizer SR)
        {
            Topic = "Estimation";
            Estimation = estimate;
            var perfer = context.Performers.Include(
                p=>p.Performer
            ).FirstOrDefault(
                p => p.PerformerId == estimate.OwnerId
            );
            // Use estimate.OwnerId;
            ProviderInfo = new ProviderClientInfo {
                Rate = perfer.Rate,
                UserName = perfer.Performer.UserName,
                Avatar = perfer.Performer.Avatar,
                UserId = perfer.PerformerId
            };
           ((IEvent)this).Sender = perfer.Performer.UserName;
            ((IEvent)this).Message = string.Format(SR["EstimationMessageToClient"],perfer.Performer.UserName,
            estimate.Title,estimate.GetTotal());
        }
        ProviderClientInfo ProviderInfo { get; set; }
        Estimate Estimation { get; set; }

        public string Topic
        {
            get; set;
        }

        public string Sender
        {
            get; set;
        }

        public string Message
        {
             get; set;
        }
    }
}