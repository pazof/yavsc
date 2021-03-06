using System;
using System.Collections.Generic;
using Yavsc.Services;

namespace Yavsc.Billing
{
    public interface IBillable {
        string Description { get; }
        List<IBillItem> GetBillItems();
        long Id { get; set; }

        string ActivityCode { get; set; }

        string PerformerId { get; set; }
        string ClientId { get; set; }
        /// <summary>
        /// Date de validation de la demande par le client
        /// </summary>
        /// <returns></returns>

        DateTime? ValidationDate { get; }

        bool GetIsAcquitted ();

        string GetFileBaseName (IBillingService service);

    }
}
