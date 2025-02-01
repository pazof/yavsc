using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Yavsc.Models.Payment {
    using Yavsc;
    using Relationship;
    using Yavsc.Attributes.Validation;

    public class PayPalPayment : ITrackedEntity
    {
        [YaRequired,Key]
        public string CreationToken { get; set; }

        [YaRequired]
        public string ExecutorId { get; set; }
        [ForeignKey("ExecutorId")]
        public virtual ApplicationUser Executor { get; set; }

        public string PaypalPayerId { get; set; }

        public string OrderReference { get; set; }
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

        public string State { get; set; }

        public bool IsOk() { return State == "SUCCESS"; }
        public virtual List<HyperLink> Links { get; set; }
    }
}
