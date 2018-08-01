
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
    using Yavsc.Abstract.Workflow;
    using Yavsc.Services;

    public abstract class NominativeServiceCommand : IBaseTrackedEntity, IDecidableQuery, IIdentified<long>
  {
        public string GetInvoiceId() { return GetType().Name +  "/" + Id;  }

        public abstract long Id { get; set; }
        public abstract string Description { get; set; }

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


    [Display(Name="Previsional")]
    public decimal? Previsional { get; set; }
    /// <summary>
    /// The bill
    /// </summary>
    /// <returns></returns>

        [Required]
        public string ActivityCode { get; set; }

        [ForeignKey("ActivityCode"),JsonIgnore,Display(Name="Domaine d'activité")]
        public virtual Activity Context  { get; set ; }

        public bool Rejected { get; set; }

        public DateTime RejectedAt { get; set; }

        public abstract System.Collections.Generic.List<IBillItem> GetBillItems();

        public bool GetIsAcquitted()
        {
            return Regularisation?.IsOk() ?? false;
        }

        public string GetFileBaseName(IBillingService billingService)
        {
            string type = GetType().Name;
            string ack = GetIsAcquitted() ? "-ack" : null;
            var bcode = billingService.BillingMap[type];
            return $"facture-{bcode}-{Id}{ack}";
        }
        
        [Display(Name = "PaymentId")]
        public string PaymentId { get; set; }

        [ForeignKey("PaymentId"), Display(Name = "Acquittement de la facture")]
        public virtual PayPalPayment Regularisation { get; set; }

    }
}
