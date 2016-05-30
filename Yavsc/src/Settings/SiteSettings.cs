namespace Yavsc
{
    public class SiteSettings
    {
        public string Title { get; set; }
        public string Slogan { get; set; }
        /// <summary>
        /// Conceptually, 
        /// This authorisation server only has this present site as unique audience. 
        /// </summary>
        /// <returns></returns>
        public string Audience { get; set; }
        /// <summary>
        /// it's a very small company, with one domaine name only,
        /// so let it be the same as in the Audience field.
        /// </summary>
        /// <returns></returns>
        public string Authority { get; set; }
        public EmailEntry Owner { get; set; }
        public EmailEntry Admin { get; set; }
        public ThirdPartyFiles UserFiles { get; set; }

    }
}
