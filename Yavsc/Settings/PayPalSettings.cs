namespace  Yavsc
{
    /// <summary>
    /// PayPal NV/SOAP API Credentials
    /// </summary>
    public class PayPalSettings {
        /// <summary>
        /// supported values: <c>sandbox</c> or <c>production</c>
        /// </summary>
        /// <returns></returns>
        public string Mode { get; set; }
        /// <summary>
        ///
        /// </summary>
        public string ClientId { get; set; }

        /// <summary>
        /// For sandbox only?
        /// </summary>
        /// <returns></returns>
        public string ClientSecret { get; set; }
        /// <summary>
        /// Your application id ...
        /// </summary>
        /// <returns></returns>
        public string ApplicationId { get; set; }

        /// <summary>
        /// Get it from your PayPal Business profile settings
        /// </summary>
        /// <returns></returns>
        public string MerchantAccountId { get; set; }

        /// <summary>
        /// Merchant PayPal Classic Api user name
        /// </summary>
        /// <returns></returns>
        public string MerchantAccountUserName { get; set; }


        /// <summary>
        /// PayPal Classic Api credentials model
        /// </summary>
        public class ClassicPayPalAccountApiCredential  {
            public string Signature { get; set; }
            public string ApiUsername { get; set; }
            public string ApiPassword { get; set; }
        }

        /// <summary>
        ///  Live access is configured at:
        ///  https://www.paypal.com/businessprofile/mytools/apiaccess
        /// </summary>
        public ClassicPayPalAccountApiCredential[] Accounts { get; set; }

    }
}
