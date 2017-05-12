using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Yavsc.Models.Payment {
    using YavscLib;

    public class PaypalPayment : IBaseTrackedEntity
    {
        [Required,Key]
        public string PaypalPaymentId { get; set; }

        [Required]
        public string ExecutorId { get; set; }
        [ForeignKey("ExecutorId")]
        public virtual ApplicationUser Executor { get; set; }

        [Required]
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
    }
}



