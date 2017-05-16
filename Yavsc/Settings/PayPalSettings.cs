namespace  Yavsc
{
    public class PayPalSettings {

        public string Mode { get; set; }
        public string Secret { get; set; }
        public string ClientId { get; set; }

        // NV/SOAP Api - Signature
        public string UserId { get; set; }
        public string Password { get; set; }
        public string Signature { get; set; }
    }
}
