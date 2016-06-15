
using System;

namespace Yavsc.Models.Billing
{
    public partial class EstimateAgreement : RDVEstimate
    {
        public DateTime ClientValidationDate { get; set; }
    }
}