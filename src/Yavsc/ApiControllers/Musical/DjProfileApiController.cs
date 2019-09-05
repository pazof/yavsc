namespace Yavsc.ApiControllers
{
    using Models;
    using Models.Musical.Profiles;

    public class DjProfileApiController : ProfileApiController<DjSettings>
    {
        public DjProfileApiController(ApplicationDbContext context) : base(context)
        {
        }
    }
}