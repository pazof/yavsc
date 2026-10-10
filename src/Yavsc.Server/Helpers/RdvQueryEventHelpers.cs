using Yavsc.Abstract.Identity;
using Yavsc.Models;
using Yavsc.Models.Billing;
using Yavsc.Models.Messaging;
using Yavsc.Models.Workflow;

namespace Yavsc.Helpers;

public static class RdvQueryEventHelpers
{
    public static RdvQueryEvent CreateEvent(this RdvQuery query, string subtopic)
        => new(subtopic)
        {
            Sender = query.ClientId,
            Reason = query.Reason,
            Client = new ClientProviderInfo
            {
                UserName = query.Client.UserName,
                UserId = query.ClientId,
                Avatar = query.Client.Avatar
            },
            Previsional = query.Provisional,
            EventDate = query.EventDate,
            Location = query.Location,
            Id = query.Id,
            ActivityCode = query.ActivityCode,
            BillingCode = BillingCodes.Rdv
        };
}
