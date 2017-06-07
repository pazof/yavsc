using Yavsc.Controllers.Generic;
using Yavsc.Models;
using Yavsc.Models.Workflow.Profiles;
using Yavsc.Services;

namespace Yavsc.Controllers
{
    public class FormationSettingsController : SettingsController<FormationSettings>
    {

        public FormationSettingsController(ApplicationDbContext context, IBillingService billing) : base(context, billing)
        {
        }

    }
}
