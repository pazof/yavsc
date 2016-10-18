using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookAStar.Data
{
    public enum LocalState
    {
        UpToDate = 0,
        New,
        Edited,
        Removed
    }
}
