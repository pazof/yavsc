using Microsoft.Extensions.Localization;
using System.Linq;

using Yavsc.Interfaces.Workflow;
using Yavsc.Models.Haircut;
using Yavsc.ViewModels.PayPal;
using Yavsc.Helpers;

namespace Yavsc.Models.HairCut
{
    public class HairCutPayementEvent: IEvent
    {
        public HairCutPayementEvent(string sender, PaymentInfo info, HairCutQuery query, IStringLocalizer localizer)
        {
            Sender = sender;
            this.query = query;
            invoiceId = info.DetailsFromPayPal.GetExpressCheckoutDetailsResponseDetails.InvoiceID;
            payerName = info.DetailsFromPayPal.GetExpressCheckoutDetailsResponseDetails.PayerInfo.Address.Name;
            phone = info.DetailsFromPayPal.GetExpressCheckoutDetailsResponseDetails.PayerInfo.Address.Phone;
            payerEmail = info.DetailsFromPayPal.GetExpressCheckoutDetailsResponseDetails.PayerInfo.Payer;
            amount = string.Join(", ",
                info.DetailsFromPayPal.GetExpressCheckoutDetailsResponseDetails.PaymentDetails.Select(
                    p => $"{p.OrderTotal.value} {p.OrderTotal.currencyID}"));
             gender = query.Prestation.Gender;
             date = query.EventDate?.ToString("dd MM yyyy hh:mm");
             lieu = query.Location.Address;
             clientFinal = (gender == HairCutGenders.Women) ? localizer["Women"] +
                " " + localizer[query.Prestation.Length.ToString()] : localizer[gender.ToString()];
             token = info.DetailsFromPayPal.GetExpressCheckoutDetailsResponseDetails.Token;
             payerId = info.DetailsFromPayPal.GetExpressCheckoutDetailsResponseDetails.PayerInfo.PayerID;
        }

        public string Topic => "/topic/HaircutPayment";

        public string Sender { get; set; }

        HairCutQuery query;

        private string invoiceId;
        private string payerName;
        private string phone;
        private string payerEmail;
        private string amount;
        private HairCutGenders gender;
        private string date;
        private string lieu;
        private string clientFinal;
        private string token;
        private string payerId;

        public string CreateBody()
        {
            return $@"# Paiment confirmé: {amount}

Effectué par : {payerName} [{payerEmail}]
Identifiant PayPal du paiment: {token}
Identifiant PayPal du payeur: {payerId}
Identifiant de la facture sur site: {invoiceId}


# La prestation concernée:

Demandeur: {query.Client.UserName}

Date: {date}

Lieu: {lieu}

Le client final: {clientFinal}

{query.GetBillText()}

";
        }
    }
}