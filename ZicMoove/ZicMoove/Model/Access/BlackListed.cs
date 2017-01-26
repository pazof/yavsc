using ZicMoove.Model.Access;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yavsc.Models;

namespace ZicMoove.Model.Access
{
    public class BlackListed : IBlackListed
    {
        public long Id
        {
            get; set;
        }

        public string OwnerId
        {
            get; set;
        }

        public string UserId
        {
            get; set;
        }
    }
}
