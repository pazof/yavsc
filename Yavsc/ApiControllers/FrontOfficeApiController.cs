using System.Collections.Generic;
using Microsoft.AspNet.Mvc;
using Yavsc.Helpers;
using Yavsc.Models;
using Yavsc.Models.Workflow;

namespace Yavsc.ApiControllers
{
    [Route("api/front")]
    public class FrontOfficeApiController: Controller
    {
        ApplicationDbContext dbContext;
        public FrontOfficeApiController(ApplicationDbContext context)
        {
            dbContext = context;
        }

	[HttpGet,Route("Profiles/{actCode}")]
        IEnumerable<PerformerProfile> Profiles (string actCode)
        {
            return dbContext.ListPerformers(actCode);
        }
    }
}
