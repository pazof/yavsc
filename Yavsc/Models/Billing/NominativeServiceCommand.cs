
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Yavsc.Models.Billing
{
    using Newtonsoft.Json;
    using Workflow;
    using YavscLib;
    using YavscLib.Billing;
    using YavscLib.Models.Workflow;

    public abstract class NominativeServiceCommand : IBaseTrackedEntity, IQuery, IIdentified<long>
  {
        public abstract long Id { get; set; }
        public abstract string Description { get; set; }
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

        public System.Collections.Generic.List<IBillItem> GetBillItems()
        {
            throw new NotImplementedException();
        }
    }
}
