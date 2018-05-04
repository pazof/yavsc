using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Yavsc.Models.Access
{
    public class BanByEmail
    {
        [Required]
        public long BanId { get; set; }
        
        [ForeignKey("BanId")]
        public virtual Ban UserBan  { get; set; }

        [Required]
        public string email { get; set; }

    }
}