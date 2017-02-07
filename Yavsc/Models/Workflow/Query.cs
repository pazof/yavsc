using System;

namespace Yavsc.Models.Workflow
{
  using Interfaces.Workflow;
  using Models.Market;
  using YavscLib;

    public class Query<P>: IBaseTrackedEntity where P : BaseProduct
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
