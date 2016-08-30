
namespace Yavsc.Models.Market {
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations.Schema;

    public partial class Service : BaseProduct
    {
        public string ContextId { get; set; }
        [ForeignKey("ContextId")]
        public virtual Activity Context { get; set; }

        public List<IBillingClause> Billing { get; set; }
        
    }

}