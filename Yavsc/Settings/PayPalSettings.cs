namespace  Yavsc
{
    public class PayPalSettings {

        public string Mode { get; set; }
        public string Secret { get; set; }
        public string ClientId { get; set; }

        // NV/SOAP Api - Signature
        public string APIUserId { get; set; }
        public string APIPassword { get; set; }
        public string APISignature { get; set; }
    }
}
