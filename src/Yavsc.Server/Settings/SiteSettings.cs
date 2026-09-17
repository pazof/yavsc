#nullable enable annotations

using Yavsc.Models.Relationship;

namespace Yavsc
{
    public class SiteSettings
    {
        public string Title { get; set; }

        public string Slogan { get; set; }
        public string Banner { get; set; }

        public string StyleSheet { get; set; }
        public string FavIcon { get; set; }
        public string Logo { get; set; }
        /// <summary>
        /// Origins to allow via the CORS "default" policy.
        /// Each entry must be a full origin (scheme + host [+ port]), e.g. "https://app.example.com".
        /// </summary>
        public IList<string> CorsAllowedOrigins { get; set; }

        /// <summary>
        /// External Url
        /// </summary>
        /// <value></value>
        public string ExternalUrl { get; set; }
        /// <summary>
        /// Base URL of the API fronting this site.
        /// </summary>
        public string ApiUrl { get; set; }
        /// <summary>
        /// Must be a fqdn.
        /// </summary>
        /// <returns></returns>
        public string Authority { get; set; }
        /// <summary>
        /// Owner's email
        /// </summary>
        /// <returns></returns>
        public StaticContact Owner { get; set; }

        /// <summary>
        /// Administrator's email
        /// </summary>
        /// <returns></returns>
        public StaticContact Admin { get; set; }

        public string DataDir { get; set; }
        public string Avatars { get; set; }
        public long Quota { get; set; }
        public string Blog { get; set; }
        public string Bills { get; set; }
        public string GitRepository { get; set; }

        /// <summary>
        /// Specifies the directory where should be
        /// generated pdf files using pandoc
        ///  </summary>
        /// <returns>The temporary directory to use</returns>
        public string TempDir { get; set; }


        /// <summary>
        /// Disk usage user list maximum length in memory
        /// </summary>
        /// <value></value>
        public int DUUserListLen { get; set; }

        /// <summary>
        /// Default acl file name
        /// </summary>
        /// <value></value>
        public string AccessListFileName { get; set; }

    }
}
