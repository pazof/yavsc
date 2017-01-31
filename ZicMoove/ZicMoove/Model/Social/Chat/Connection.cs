using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YavscLib;

namespace ZicMoove.Model.Social.Chat
{
    public class Connection : IConnection
    {
        public bool Connected
        {
            get; set;
        }

        public string ConnectionId
        {
            get; set;
        }

        public string UserAgent
        {
            get; set;
        }
    }
}
