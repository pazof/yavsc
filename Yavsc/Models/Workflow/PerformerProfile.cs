using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Yavsc.Models.Workflow 
{
    using Models.Relationship;

    public class PerformerProfile {

        [Key]
        public string PerformerId { get; set; }
        [ForeignKey("PerformerId")]
        public virtual ApplicationUser Performer { get; set; }

        [InverseProperty("User")]
        [Display(Name="Activity")]
        public virtual List<UserActivity> Activity { get; set; }

        [Required,StringLength(14),Display(Name="SIREN"),
        RegularExpression(@"^[0-9]{9,14}$", ErrorMessage = "Only numbers are allowed here")]
        public string SIREN { get; set; }

        public long OrganizationAddressId { get; set; }

        [Required,Display(Name="Organization address"),ForeignKey("OrganizationAddressId")]
        public virtual Location OrganizationAddress { get; set; }

        [Display(Name="Accept notifications on client query")]
        public bool AcceptNotifications { get; set; }

        [Display(Name="Accept notifications from non-VIP users")]
        public bool AcceptPublicContact { get; set; }

        [Display(Name="Use my geo-localization, and give me clients near by me")]
        public bool UseGeoLocalizationToReduceDistanceWithClients { get; set; }

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
            return Performer?.Posts?.Count > 0 ;
        } }
        
    }
}