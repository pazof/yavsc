
namespace Yavsc.Models {
    using System.ComponentModel.DataAnnotations.Schema;

    public enum BillingMode { 
        Unitary,
        SetPrice,
        ByExecutionTime
    }
public partial class Service : BaseProduct
    {
        public string ContextId { get; set; }
        [ForeignKey("ContextId")]
        public virtual Activity Context { get; set; }

        public BillingMode? Billing { get; set; }
        // TODO public ServiceSpecification Specification { get; set; }
        /// <summary>
        /// In euro, either by hour or by release
        /// </summary>
        /// <returns></returns>
        public decimal? Pricing { get; set; }
        
        /// <summary>
        /// Associate a class of flow worker to handle with
        /// </summary>
        /// <returns></returns>
        public string Specification { get; set; }
    }

}