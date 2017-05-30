
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Yavsc.Models.Billing
{
    using Newtonsoft.Json;
    using Workflow;
    using Yavsc.Models.Payment;
    using Yavsc;
    using Yavsc.Billing;
    using Yavsc.Workflow;

    public abstract class NominativeServiceCommand : IBaseTrackedEntity, IQuery, IIdentified<long>
  {
        public string GetInvoiceId() { return GetType().Name +  "/" + Id;  }

        public abstract long Id { get; set; }
        public abstract string GetDescription ();

        [Required()]
        public bool Consent { get; set; }
        public DateTime DateCreated
        {
            get; set;
        }

        public DateTime DateModified
        {
             get; set;
        }

        public string UserCreated
        {
             get; set;
        }

        public string UserModified
        {
             get; set;
        }

        [DisplayAttribute(Name="Status de la requête")]
        public QueryStatus Status { get; set; }

    [Required]
    public string ClientId { get; set; }

    /// <summary>
    /// The client
    /// </summary>
    [ForeignKey("ClientId"),Display(Name="Client")]
    public ApplicationUser Client { get; set; }

    [Required]
    public string PerformerId { get; set; }
    /// <summary>
    /// The performer identifier
    /// </summary>
    [ForeignKey("PerformerId"),Display(Name="Préstataire")]
    public PerformerProfile PerformerProfile { get; set; }

    public DateTime? ValidationDate {get; set;}


    [Display(Name="Montant prévisionel de la préstation")]
    public decimal? Previsional { get; set; }
    /// <summary>
    /// The bill
    /// </summary>
    /// <returns></returns>

        [Required]
        public string ActivityCode { get; set; }

        [ForeignKey("ActivityCode"),JsonIgnore,Display(Name="Domaine d'activité")]
        public virtual Activity Context  { get; set ; }

        public abstract System.Collections.Generic.List<IBillItem> GetBillItems();
        public string PaymentId { get; set; }

        [ForeignKey("PaymentId"), Display(Name = "Acquittement de la facture")]
        public virtual PayPalPayment Regularisation { get; set; }
    }
}
