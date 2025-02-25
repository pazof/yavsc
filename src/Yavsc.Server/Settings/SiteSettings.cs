using Yavsc.Models.Relationship;

namespace Yavsc
{
    public class SiteSettings
    {
        public string Title { get; set; }
        public string Slogan { get; set; }

        public string StyleSheet { get; set; }
        public string FavIcon { get; set; }
        public string Logo { get; set; }
        /// <summary>
        /// Conceptually,
        /// This authorisation server only has this present site as unique audience.
        /// </summary>
        /// <returns></returns>
        public string Audience { get; set; }
        /// <summary>
        /// it's a very small company, with one domaine name only,
        /// so let it be the same as in the Audience field.
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
        public string Avatars { get; set; } = "avatars";
        public long Quota { get; set; }
        public string Blog { get; set; } = "blogs";
        public string Bills { get; set; } = "bills";
        public string GitRepository { get; set; } = "sources";

        public string BusinessName { get; set; }
        public string Street { get; set; }
        public string PostalCode { get; set; }
        public string CountryCode { get; set; }

        public string HomeViewName { get; set; }
        /// <summary>
        /// Specifies the directory where should be
        /// generated pdf files using pandoc
        ///  </summary>
        /// <returns>The temporary directory to use</returns>
        public string TempDir { get; set; } = "temp";

        /// <summary>
        /// Only one performer will capture payments
        /// </summary>
        /// <returns>user capturing payments id</returns>
        public string OnlyOnePerformerId { get; set; }

        /// <summary>
        /// Only one activity will be supported
        /// </summary>
        /// <returns>the supported activity code</returns>
        public string OnlyOneActivityCode { get; set; }

        /// <summary>
        /// Disk usage user list maximum length in memory
        /// </summary>
        /// <value></value>
        public int DUUserListLen { get; set; } = 256;

        /// <summary>
        /// Default acl file name
        /// </summary>
        /// <value></value>
        public string AccessListFileName { get; set; } = ".access";

    }
}
