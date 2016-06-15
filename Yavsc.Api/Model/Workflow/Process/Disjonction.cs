using System.Collections.Generic;

namespace Yavsc.Models
{
    public class Disjonction : List<IRequisition>, IRequisition
    {
        public bool Eval()
        {
            foreach (var req in this)
                if (req.Eval())
                    return true;
            return false;
        }
    }

}
