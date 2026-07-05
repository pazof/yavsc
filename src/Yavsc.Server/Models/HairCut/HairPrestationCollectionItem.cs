using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Yavsc.Models.Haircut
{
    public class HairPrestationCollectionItem {

        [Key(), DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }
        public long PrestationId { get; set; }
        [ForeignKeyAttribute("PrestationId")]
        public virtual HairPrestation Prestation { get; set; }

        public long QueryId { get; set; }

        [ForeignKeyAttribute("QueryId")]
        public virtual HairMultiCutQuery Query { get; set; }
    }
}
