namespace  Yavsc
{

    public class SiteSettings
    {
        public string Title { get; set; }
        public string Slogan { get; set; }
        public string Audience { get; set; }
        public string Authority { get; set; }
        public EmailEntry Owner { get; set; }
        public EmailEntry Admin { get; set; }
        public ThirdPartyFiles UserFiles { get; set; }

    }
}
