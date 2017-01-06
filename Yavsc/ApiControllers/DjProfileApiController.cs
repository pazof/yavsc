
namespace Yavsc.ApiControllers
{
    using Models;
    using Models.Booking;

    public class DjProfileApiController : ProfileApiController<DjPerformerProfile>
    {
        public DjProfileApiController(ApplicationDbContext context) : base(context)
        {
            
        }
    }
}