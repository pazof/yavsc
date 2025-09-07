using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Yavsc.Models.Relationship;

namespace Yavsc
{
    public class SiteSettings
    {
        public string Title { get; set; } = "Yavsc";
        
        public string Slogan { get; set; } = "";
        public string Banner { get; set; } = "";

        public string StyleSheet { get; set; } = "site.css";
        public string FavIcon { get; set; } = "favicon.ico";
        public string Logo { get; set; } = "logo.png";
        /// <summary>
        /// </summary>
        /// <returns></returns>
        public string Audience { get; set; } = "lua.pschneider.fr";
       
        /// <summary>
        /// External Url
        /// </summary>
        /// <value></value>
        public string ExternalUrl { get; set; } = "http://lua.pschneider.fr";
        /// <summary>
        /// Must be a fqdn.
        /// </summary>
        /// <returns></returns>
        public string Authority { get; set; } = "lua.pschneider.fr";
        /// <summary>
        /// Owner's email
        /// </summary>
        /// <returns></returns>
        public StaticContact Owner { get; set; } = new StaticContact
        {
            EMail = "root@lua.pschneider.fr",
            Name = "Root"
        } ;

        /// <summary>
        /// Administrator's email
        /// </summary>
        /// <returns></returns>
        public StaticContact Admin { get; set; } = new StaticContact
        {
            EMail = "root@lua.pschneider.fr",
            Name = "Root"
        } ;

        public string DataDir { get; set; } = "data";
        public string Avatars { get; set; } = "avatars";
        public long Quota { get; set; }
        public string Blog { get; set; } = "blogs";
        public string Bills { get; set; } = "bills";
        public string GitRepository { get; set; } = "sources";

        public string BusinessName { get; set; } = "Yavsc";
        public string? Street { get; set; }
        public string? PostalCode { get; set; }
        public string? CountryCode { get; set; }

        public string HomeViewName { get; set; } = "Home";
        /// <summary>
        /// Specifies the directory where should be
        /// generated pdf files using pandoc
        ///  </summary>
        /// <returns>The temporary directory to use</returns>
        public string TempDir { get; set; } = "temp";


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
