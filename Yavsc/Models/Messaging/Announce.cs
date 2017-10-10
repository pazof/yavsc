using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Yavsc.Interfaces;

namespace Yavsc.Models.Messaging
{
    public enum Reason { 
            Private,
            Corporate,
            SearchingAPro,
            Selling,
            Buying,
            ServiceProposal

        }
    public class Announce: BaseEvent, IOwned
    {
        public Reason For { get; set; }

        [Key,DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        public string OwnerId { get; set; }

        [ForeignKey("OwnerId")]
        public virtual ApplicationUser Owner { get; set; } 
    }
}