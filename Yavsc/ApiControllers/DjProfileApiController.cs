

namespace Yavsc.ApiControllers
{
    using Models;
    using Models.Booking.Profiles;

    public class DjProfileApiController : ProfileApiController<DjSettings>
    {
        public DjProfileApiController(ApplicationDbContext context) : base(context)
        {
            
        }
    }
}
