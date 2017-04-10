
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace Yavsc.Models.Workflow
{
    using Market;
    using YavscLib;

    public class Activity : IBaseTrackedEntity, IActivity
    {

        [StringLength(512), Required, Key]
        [Display(Name = "Code")]
        public string Code { get; set; }
        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        [StringLength(512), Required()]
        [Display(Name = "Nom")]
        public string Name { get; set; }

        [StringLength(512)]
        [Display(Name = "Code du parent")]
        public string ParentCode { get; set; }

        [ForeignKey("ParentCode"), JsonIgnore]
        [Display(Name = "Activité parent")]
        public virtual Activity Parent { get; set; }

        [InverseProperty("Parent"), JsonIgnore]
        [Display(Name = "Activités filles")]
        public virtual List<Activity> Children { get; set; }

        [Display(Name = "Description")]
        public string Description { get; set; }

        [Display(Name = "Photo")]
        public string Photo { get; set; }

        [InverseProperty("Context")]
        [DisplayAttribute(Name = "Services liés")]
        public List<Service> Services { get; set; }

        /// <summary>
        /// Moderation settings
        /// </summary>
        /// <returns></returns>
        [DisplayAttribute(Name = "Groupe de modération")]
        public string ModeratorGroupName { get; set; }

        /// <summary>
        /// indice de recherche de cette activité
        /// rendu par le système.
        /// Valide entre 0 et 100,
        /// Il démarre à 0.
        /// </summary>
        [Range(0, 100)][DisplayAttribute(Name = "Indice d'exposition")]
        [DisplayFormatAttribute(DataFormatString="{0}%")]
        public int Rate { get; set; }
        [DisplayAttribute(Name = "Classe de paramétrage")]
        public string SettingsClassName { get; set; }

        [InverseProperty("Context")]
        [Display(Name="Formulaires de commande")]
        public virtual List<CommandForm> Forms { get; set; }

        [Display(Name="Date de création")]
        public DateTime DateCreated
        {
            get; set;
        }

        [Display(Name="Createur")]
        public string UserCreated
        {
            get; set;
        }

        [Display(Name="Date de dernière modification")]
        public DateTime DateModified
        {
            get; set;
        }

        [Display(Name="Utilisateur ayant modifié le dernier")]
        public string UserModified
        {
            get; set;
        }

        [Display(Name="Caché")]

        public bool Hidden { get; set; }

    }
}
