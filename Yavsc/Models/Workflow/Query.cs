using System;
using Yavsc.Interfaces.Workflow;
using Yavsc.Models.Market;

namespace Yavsc.Models.Workflow
{

    public class Query<P>: IBaseTrackedEntity  where P : BaseProduct
    {
        public DateTime DateCreated
        {
            get; set;
        }

        public DateTime DateModified
        {
             get; set;
        }

        public string UserCreated
        {
             get; set;
        }

        public string UserModified
        {
             get; set;
        }

        QueryStatus Status { get; set; }
    }

}
