
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;
using Yavsc.Models.Market;

namespace Yavsc.Models.Workflow
{
    public class Activity : IBaseTrackedEntity
    {

        [StringLength(512),Required,Key]
        public string Code {get; set;}
        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        [StringLength(512),Required()]
        public string Name {get; set;}

        [StringLength(512)]
        public string ParentCode { get; set; }

        [ForeignKey("ParentCode"),JsonIgnore]
        public virtual Activity Parent { get; set; }

        [InverseProperty("Parent"),JsonIgnore]
        public virtual List<Activity> Children { get; set; }

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

        /// <summary>
        /// indice de recherche de cette activité
        /// rendu par le système.
        /// Valide entre 0 et 100,
        /// Il démarre à 0.
        /// </summary>
        [Range(0,100)]
        public int Rate { get; set; }
        [DisplayAttribute(Name="SettingsClass")]
        public string SettingsClassName { get; set; }

        [InverseProperty("Context")]
        public virtual List<CommandForm> Forms { get; set; }

        public DateTime DateCreated
        {
            get; set; 
        }

        public string UserCreated
        {
            get; set; 
        }

        public DateTime DateModified
        {
            get; set; 
        }

        public string UserModified
        {
            get; set; 
        }
    }
}
