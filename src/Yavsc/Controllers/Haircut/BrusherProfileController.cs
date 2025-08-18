using Yavsc.Models;
using Yavsc.Models.Haircut;
using Microsoft.AspNetCore.Authorization;
using Yavsc.Controllers.Generic;

namespace Yavsc.Controllers
{
    [Authorize("Performer")]
    public class BrusherProfileController : SettingsController<BrusherProfile>
    {
        public BrusherProfileController(ApplicationDbContext context) : base(context)
        {

        }

    }
}
