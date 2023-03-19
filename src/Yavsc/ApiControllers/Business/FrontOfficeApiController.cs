using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Yavsc.Helpers;
using Yavsc.Models;
using Yavsc.Services;
using Yavsc.ViewModels.FrontOffice;

namespace Yavsc.ApiControllers
{
    [Route("api/front")]
    public class FrontOfficeApiController : Controller
    {
        ApplicationDbContext dbContext;

        private IBillingService billing;

        public FrontOfficeApiController(ApplicationDbContext context, IBillingService billing)
        {
            dbContext = context;
            this.billing = billing;
        }

        [HttpGet("profiles/{actCode}")]
        IEnumerable<PerformerProfileViewModel> Profiles(string actCode)
        {
            return dbContext.ListPerformers(billing, actCode);
        }

        [HttpPost("query/reject")]
        public IActionResult RejectQuery(string billingCode, long queryId)
        {
            if (billingCode == null) return BadRequest("billingCode");
            if (queryId == 0) return BadRequest("queryId");
            var billing = BillingService.GetBillable(dbContext, billingCode, queryId);
            if (billing == null) return BadRequest();
            billing.Rejected = true;
            billing.RejectedAt = DateTime.Now;
            dbContext.SaveChanges();
            return Ok();
        }
    }
}
