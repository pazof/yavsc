using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace Yavsc.Models.Chat
{
    using Yavsc;

    public class Connection : IConnection
    {
        [JsonIgnore,Required]
        public string ApplicationUserId { get; set; }
        [ForeignKey("ApplicationUserId"),JsonIgnore]
        public virtual ApplicationUser Owner { get; set; }

        [Key]
        public string ConnectionId { get; set; }
        public string UserAgent { get; set; }
        public bool Connected { get; set; }
    }

}
