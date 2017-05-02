using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Yavsc.Models.Market {
    using System.ComponentModel.DataAnnotations;
    using Workflow;
    using Yavsc.Models.Billing;

    public class Service : BaseProduct
    {
                /// <summary>
        /// An unique product identifier.
        /// </summary>
        /// <returns></returns>
        [Key(),DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

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
