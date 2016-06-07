using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Yavsc.Models {

    public class PerformerProfile {

        [Key]
        public string PerfomerId { get; set; }
        [ForeignKey("PerfomerId"),Display(Name="Performer")]
        public virtual ApplicationUser Performer { get; set; }

        [Display(Name="Activity"),Required]
        public string ActivityCode { get; set; }

        public Service Offer { get; set; }

        [Required,StringLength(14),Display(Name="SIREN"),
        RegularExpression(@"^[0-9]{9,14}$", ErrorMessage = "Only numbers are allowed here")]
        public string SIREN { get; set; }

        public long OrganisationAddressId { get; set; }

        [Required,Display(Name="Organisation address"),ForeignKey("OrganisationAddressId")]
        public virtual Location OrganisationAddress { get; set; }

        [ForeignKey("ActivityCode"),Display(Name="Activity")]
        public virtual Activity Activity { get; set; }

        [Display(Name="Accept notifications on client query")]
        public bool AcceptNotifications { get; set; }

        [Display(Name="Accept notifications from non-VIP users")]
        public bool AcceptPublicContact { get; set; }

        [Display(Name="Allow my geolocatisation, nearby my clients")]
        public bool AcceptGeoLocalisation { get; set; }

        [Display(Name="Web site")]
        public string WebSite { get; set; }

        [Display(Name="Active")]
        public bool Active { get; set; }

        [Display(Name="Maximal Daily Cost (euro/day)")]
        public int? MaxDailyCost { get; set; }

        [Display(Name="Minimal Daily Cost (euro/day)")]
        public int? MinDailyCost { get; set; }

        [Display(Name="Rate from clients")]
        public int Rate { get; set; }

        [NotMapped]
        public bool DoesBlog { get {
            return Performer?.Posts != null ? Performer.Posts.Count > 0 : false;
        } }

    }
}