using System.Linq;
using Microsoft.Extensions.Localization;
using Yavsc.Helpers;
using Yavsc.Models.Billing;

namespace Yavsc.Models.Messaging
{
    public class EstimationEvent: IEvent
    {
        public EstimationEvent(ApplicationDbContext context, Estimate estimate, IStringLocalizer SR)
        {
            Estimation = estimate;
            var perfer = context.Performers.FirstOrDefault(
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

        private string subtopic = null;
        string IEvent.Topic
        {
            get
            {
                return "/topic/estimate"+subtopic!=null?"/"+subtopic:"";
            }

            set
            {
                subtopic = value;
            }
        }

        string IEvent.Sender
        {
           get; set;
        }

        string IEvent.Message
        {
            get; set;
        }
    }
}