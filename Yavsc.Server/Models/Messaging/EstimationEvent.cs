using Microsoft.Extensions.Localization;

namespace Yavsc.Models.Messaging
{
    using Interfaces.Workflow;
    using Billing;
    using Yavsc.Helpers;
    using Yavsc.Models.Workflow;

    public class EstimationEvent: IEvent
    {
        public EstimationEvent( Estimate estimate, IStringLocalizer SR)
        {
            Topic = "Estimation";
            Estimation = estimate;
            perfer = estimate.Owner;

            ProviderInfo = new ProviderClientInfo {
                Rate = perfer.Rate,
                UserName = perfer.Performer.UserName,
                Avatar = perfer.Performer.Avatar,
                UserId = perfer.PerformerId
            };
           Sender = perfer.Performer.UserName;
            _localizer = SR;
        }
          // TODO via e-mail only: Message = string.Format(
              // SR["EstimationMessageToClient"],perfer.Performer.UserName, estimate.Title,estimate.Bill.Addition());
              //
        ProviderClientInfo ProviderInfo { get; set; }
        Estimate Estimation { get; set; }

        private PerformerProfile perfer;

        public string Topic
        {
            get; set;
        }

        public string Sender
        {
            get; set;
        }

        private IStringLocalizer _localizer;

        public string CreateBody()
        {
            return string.Format( _localizer["EstimationMessageToClient"], perfer.Performer.UserName, this.Estimation.Bill.Addition());
        }
    }
}
