using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookAStar.Model.Social.Messaging
{
    public class UserMessage
    {
        public DateTime Date { get; set; }
        public string SenderId { get; set; }
        public string Message { get; set; }
    }
}
