using Yavsc.Controllers.Generic;
using Yavsc.Models;
using Yavsc.Models.Workflow.Profiles;

namespace Yavsc.Controllers
{
    public class FormationSettingsController : SettingsController<FormationSettings>
    {

        public FormationSettingsController(ApplicationDbContext context) : base(context)
        {
            
        }

    }
}
