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
        /// <summary>
        /// Owner's email
        /// </summary>
        /// <returns></returns>
        public EmailEntry Owner { get; set; }
        /// <summary>
        /// Administrator's email
        /// </summary>
        /// <returns></returns>
        public EmailEntry Admin { get; set; }
        /// <summary>
        /// User's files directory
        /// </summary>
        /// <returns></returns>
        public ThirdPartyFiles UserFiles { get; set; } 

        public string BusinessName { get; set; } 
        public string Street { get; set; } 
        public string PostalCode { get; set; } 
        public string CountryCode { get; set; } 
        /// <summary>
        /// Specifies the directory where should be 
        /// generated pdf files using pandoc
        ///  </summary>
        /// <returns>The temporary directory to use</returns>
        public string TempDir { get; set; } = "Temp";

    }
}
