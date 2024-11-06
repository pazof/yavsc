using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Yavsc.Models.Auth
{
    public class Client 
    {
        [Key][Required][DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string Id { get; set; }

        [Required]
        [MaxLength(128)]
        public string DisplayName { get; set; }
        
        [MaxLength(512)]
        public string? RedirectUri { get; set; }
        

        [MaxLength(512)]
        public string LogoutRedirectUri { get; set; }
        [MaxLength(512)]
        public string Secret { get; set; }
        public ApplicationTypes Type { get; set; }

        public bool Active { get; set; }
        public int RefreshTokenLifeTime { get; set; }
        public int AccessTokenLifetime { get; set; }
    }
}
