using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc;
using Yavsc.Abstract.Workflow;
using Yavsc.Helpers;
using Yavsc.Models;
using Yavsc.Models.Workflow;
using Yavsc.Workflow;

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

	[HttpGet,Route("profiles/{actCode}")]
        IEnumerable<PerformerProfile> Profiles (string actCode)
        {
            return dbContext.ListPerformers(actCode);
        }

        [HttpPost,Route("query/reject")]
        public IActionResult RejectQuery (string billingCode, long queryId)
        {
            if (billingCode==null) return HttpBadRequest("billingCode");
            if (queryId==0) return HttpBadRequest("queryId");
            var billing =  Startup.GetBillable(dbContext, billingCode, queryId);
            if (billing==null) return HttpBadRequest();
            billing.Rejected = true;
            billing.RejectedAt = DateTime.Now;
            dbContext.SaveChanges();
            return Ok();
        }
    }
}
