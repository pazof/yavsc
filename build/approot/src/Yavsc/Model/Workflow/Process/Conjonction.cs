using System.Collections.Generic;

namespace Yavsc.Models
{
    public class Conjonction : List<IRequisition>, IRequisition
    {
        public bool Eval()
        {
            foreach (var req in this)
                if (!req.Eval())
                    return false;
            return true;
        }
    }

}
