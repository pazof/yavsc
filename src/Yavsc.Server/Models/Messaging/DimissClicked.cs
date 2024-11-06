using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Yavsc.Attributes.Validation;

namespace Yavsc.Models.Messaging
{
    public class DismissClicked
    {
        [YaRequired]
        public string UserId { get; set; }
        
        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; }

        [YaRequired]
        public long NotificationId { get; set; }

        [ForeignKey("NotificationId")]
        public virtual Notification Notified { get; set; }

    }
}
