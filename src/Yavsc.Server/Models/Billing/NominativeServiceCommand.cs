#nullable enable annotations

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

    public abstract class NominativeServiceCommand : IQuery, IIdentified<long>, IBillable
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
    [ForeignKey("PerformerId"),Display(Name="PerformerProfile")]
    public PerformerProfile PerformerProfile { get; set; }

    public DateTime? ValidationDate {get; set;}


    [Display(Name="Provisional")]
    public decimal? Provisional { get; set; }
    /// <summary>
    /// The bill
    /// </summary>
    /// <returns></returns>

        [Required]
        public string ActivityCode { get; set; }

        [ForeignKey("ActivityCode"),JsonIgnore,Display(Name="Domaine d'activité")]
        public virtual Activity Context  { get; set ; }

        public abstract System.Collections.Generic.List<IBillItem> GetBillItems();

        /// <summary>
        /// Null until an invoice total has been established; never inferred
        /// from a provisional price or a payment gateway acknowledgement.
        /// </summary>
        public decimal? AmountDue { get; private set; }

        public decimal AmountPaid { get; private set; }

        public long? InvoiceEstimateId { get; private set; }
        public DateTimeOffset? InvoicedAtUtc { get; private set; }
        [MaxLength(3)]
        public string? InvoiceCurrency { get; private set; }
        public string? InvoiceClientId { get; private set; }
        public string? InvoicePerformerId { get; private set; }
        [JsonIgnore, System.Text.Json.Serialization.JsonIgnore]
        public string? InvoiceLinesJson { get; private set; }
        [ConcurrencyCheck]
        public Guid PaymentRevision { get; private set; }

        public void EstablishInvoice(long estimateId, string currency, IReadOnlyList<InvoiceLineResponse> lines)
        {
            ArgumentNullException.ThrowIfNull(lines);
            if (InvoiceEstimateId.HasValue || AmountDue.HasValue || AmountPaid != 0m)
                throw new InvalidOperationException("A balance or invoice already exists for this command.");
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(estimateId);
            if (currency != "EUR" || lines.Count == 0
                || lines.Any(line => line.Count <= 0 || decimal.Round(line.UnitaryCost, 2) != line.UnitaryCost))
                throw new ArgumentException("An invoice requires EUR lines with positive quantities.");
            var due = lines.Sum(line => checked(line.Count * line.UnitaryCost));
            SetPaymentAmounts(due, 0m);
            InvoiceEstimateId = estimateId;
            InvoiceCurrency = currency;
            InvoiceClientId = ClientId;
            InvoicePerformerId = PerformerId;
            var now = DateTimeOffset.UtcNow;
            InvoicedAtUtc = new DateTimeOffset(now.UtcTicks - now.UtcTicks % 10, TimeSpan.Zero);
            InvoiceLinesJson = System.Text.Json.JsonSerializer.Serialize(lines);
        }

        public List<IBillItem> GetInvoiceBillItems()
        {
            if (InvoiceLinesJson is null || InvoiceCurrency is null)
                throw new InvalidOperationException("The invoice has no frozen lines.");
            var lines = System.Text.Json.JsonSerializer.Deserialize<List<InvoiceLineResponse>>(InvoiceLinesJson)
                ?? throw new InvalidOperationException("The invoice lines are invalid.");
            return lines.Select(line => (IBillItem)new CommandLine
            {
                Name = line.Name, Description = line.Description,
                Count = line.Count, UnitaryCost = line.UnitaryCost, Currency = InvoiceCurrency
            }).ToList();
        }

        [NotMapped]
        public PaymentSummary? PaymentSummary => AmountDue.HasValue
            ? new PaymentSummary(AmountDue.Value, AmountPaid)
            : null;

        public void SetPaymentAmounts(decimal amountDue, decimal amountPaid)
        {
            if (InvoiceEstimateId.HasValue && AmountDue != amountDue)
                throw new InvalidOperationException("An established invoice total cannot be changed.");
            var summary = new PaymentSummary(amountDue, amountPaid);
            AmountDue = summary.AmountDue;
            AmountPaid = summary.AmountPaid;
            PaymentRevision = Guid.NewGuid();
        }

        public bool GetIsAcquitted()
        {
            return PaymentSummary?.IsAcquitted ?? false;
        }

        public string GetFileBaseName(IBillingService billingService)
        {
            string type = GetType().Name;
            string ack = GetIsAcquitted() ? "-ack" : null;
            var bcode = billingService.BillingMap[type];
            return $"facture-{bcode}-{Id}{ack}";
        }

        [ForeignKey("Regularization")]

        public string? PaymentId { get; set; }

        [Display(Name = "Acquittement de la facture")]
        public virtual PayPalPayment Regularization { get; set; }
    }
}
