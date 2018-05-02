using System;

namespace Yavsc.Attributes
{
    public class ActivityBillingAttribute : Attribute
    {
        public string BillingCode { get; private set; }

        /// <summary>
        /// Identifie une entité de facturation
        /// </summary>
        /// <param name="billingCode">Code de facturation,
        /// Il doit avoir une valeur unique par usage.
        /// </param>
        public ActivityBillingAttribute(string billingCode) {
            BillingCode = billingCode;
        }
    }
}