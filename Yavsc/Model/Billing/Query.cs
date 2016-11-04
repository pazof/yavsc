using Yavsc.Interfaces.Workflow;
using Yavsc.Models.Market;

namespace Yavsc.Models.Billing
{

    public class Query<P> where P : BaseProduct
    {
        QueryStatus Status { get; set; }
    }

}
