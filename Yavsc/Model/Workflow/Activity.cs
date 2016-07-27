
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Yavsc.Models.Market;

namespace Yavsc.Models
{
    public class Activity
    {

        [StringLength(512),Required,Key]
        public string Code {get; set;}
        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        [StringLength(512),Required()]
        public string Name {get; set;}

        public string Description {get; set;}
        /// <summary>
        /// Name to associate to a performer in this activity domain
        /// </summary>
        /// <returns></returns>
        public string ActorDenomination {get; set;}

        public string Photo {get; set;}

       [InverseProperty("Context")]
        public List<Service> Services { get; set; }
        
        /// <summary>
        /// Moderation settings
        /// </summary>
        /// <returns></returns>
        string ModeratorGroupName { get; set; }

    }
}
