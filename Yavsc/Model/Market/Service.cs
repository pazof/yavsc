
namespace Yavsc.Models.Market {
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations.Schema;

    public partial class Service : BaseProduct
    {
        public string ContextId { get; set; }
        [ForeignKey("ContextId")]
        public virtual Activity Context { get; set; }

        /// <summary>
        /// List of billing clause,
        /// associated to this service, 
        /// that defines the transformation rules
        /// to take in account during the transformation
        /// of a corresponding prestation to an amount to pay.
        /// This property is built at constructing a new instance
        /// and is not mapped in database. 
        /// For the moment, it's hard coded only.
        /// </summary>
        /// <returns></returns>

        [NotMapped]
        public List<IBillingClause> Billing { get; set; }
        
    }

}