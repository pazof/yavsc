using Yavsc.Models;
using Yavsc.Models.Haircut;
using Microsoft.AspNet.Authorization;
using Yavsc.Controllers.Generic;
using Yavsc.Services;

namespace Yavsc.Controllers
{
    [Authorize(Roles="Performer")]
    public class BrusherProfileController : SettingsController<BrusherProfile>
    {

        public BrusherProfileController(ApplicationDbContext context, IBillingService billing) : base(context, billing)
        {
        }

    }
}
