using Yavsc.Models;
using Yavsc.Models.Booking;
using Yavsc.Models.Workflow;

namespace Yavsc.ApiControllers
{
    public class DjProfileApiController : ProfileApiController<DjPerformerProfile>
    {
        public DjProfileApiController(ApplicationDbContext context) : base(context)
        {
            
        }
    }
}