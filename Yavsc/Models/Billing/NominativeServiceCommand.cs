
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Yavsc.Models.Billing
{
using Interfaces.Workflow;
using Workflow;
using YavscLib;

 public abstract class NominativeServiceCommand : IBaseTrackedEntity, IQuery
  {
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

        public QueryStatus Status { get; set; }

    [Required]
    public string ClientId { get; set; }

    /// <summary>
    /// The client
    /// </summary>
    [ForeignKey("ClientId")]
    public ApplicationUser Client { get; set; }

    [Required]
    public string PerformerId { get; set; }
    /// <summary>
    /// The performer identifier
    /// </summary>
    [ForeignKey("PerformerId")]
    public PerformerProfile PerformerProfile { get; set; }

    public DateTime? ValidationDate {get; set;}



    public decimal? Previsional { get; set; }
    /// <summary>
    /// The bill
    /// </summary>
    /// <returns></returns>


 }
}