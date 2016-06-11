using System.ComponentModel.DataAnnotations;

namespace Yavsc.Models.Auth
{
    public class Client 
    {
        [Key]
        public string Id { get; set; }
        public string DisplayName { get; set; }
        public string RedirectUri { get; set; }
        [MaxLength(100)]
        public string LogoutRedirectUri { get; set; }
        public string Secret { get; set; }
        public ApplicationTypes Type { get; set; }

        public bool Active { get; set; }
        public int RefreshTokenLifeTime { get; set; }

    }
}
