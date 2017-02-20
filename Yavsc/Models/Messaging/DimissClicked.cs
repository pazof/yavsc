using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Yavsc.Models.Messaging
{
    public class DimissClicked
    {
        [Required]
        public string UserId { get; set; }
        
        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; }

        [Required]
        public long NotificationId { get; set; }

        [ForeignKey("NotificationId")]
        public virtual Notification Notified { get; set; }

    }
}