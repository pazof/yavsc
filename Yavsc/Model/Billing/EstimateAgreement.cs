
using System;
using System.ComponentModel.DataAnnotations;

namespace Yavsc.Models.Billing
{
    public partial class Contract : Estimate
    {
        [Required]
        public DateTime ClientValidationDate { get; set; }
    }
}