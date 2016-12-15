using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;
using Yavsc.Models;
using YavscLib;

namespace Yavsc.Model.Chat
{

    public class Connection : IConnection
    {
        [JsonIgnore]
        public string ApplicationUserId { get; set; }
        [ForeignKey("ApplicationUserId"),JsonIgnore]
        public virtual ApplicationUser Owner { get; set; }

        [Key]
        public string ConnectionId { get; set; }
        public string UserAgent { get; set; }
        public bool Connected { get; set; }
    }
    
}