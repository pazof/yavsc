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
        public string ApplicationId { get; set; }
        public class ClassicPayPalAccountApiCredential  {
            public string Signature { get; set; }
            public string ApiUsername { get; set; }
            public string ApiPassword { get; set; }

        }
        public ClassicPayPalAccountApiCredential[] Accounts { get; set; }
    }
}
