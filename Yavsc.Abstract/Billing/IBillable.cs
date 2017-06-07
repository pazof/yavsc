using System;
using System.Collections.Generic;

namespace Yavsc.Billing
{
    public interface IBillable {
        string GetDescription ();
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

    }
}
