using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Yavsc.Models.Billing;
using Yavsc.Models.Relationship;
using Yavsc.Billing;

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

    public class HairMultiCutQuery : NominativeServiceCommand
    {
        // Bill description
        string _customDescription = null; 
        public override string Description 
        {
            get { 
                return _customDescription ?? "Prestation en coiffure à domicile [commande groupée]" ;
            }
            set {
                _customDescription = value;
            }
        } 

        [Key(), DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        override public long Id { get; set; }

        [InversePropertyAttribute("Query")]
        public virtual List<HairPrestationCollectionItem>  Prestations { get; set; }

        public Location Location { get; set; }

        public DateTime EventDate
        {
            get;
            set;
        }

        public override List<IBillItem> GetBillItems()
        {
            throw new NotImplementedException();
        }
    }
}
