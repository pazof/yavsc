using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Yavsc.Models.Payment {
    using YavscLib;

    public class PaypalPayment : IBaseTrackedEntity
    {
        [Required]
        public string ExecutorId { get; set; } 
        [ForeignKey("ExecutorId")]
        public virtual ApplicationUser Executor { get; set; }
        [Key,Required]
        string PaypalPayerId { get; set; }
        [Required]
        string PaypalPaymentId { get; set; }
        string orderReference { get; set; } 
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
    }
}



