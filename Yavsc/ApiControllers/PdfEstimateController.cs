using System;
using System.IO;
using System.Security.Claims;
using Microsoft.AspNet.Authorization;
using Microsoft.AspNet.Mvc;

namespace Yavsc.ApiControllers
{
    [Route("api/pdfestimate"), Authorize]
    public class PdfEstimateController : Controller
    {
        [HttpGet("{id}", Name = "Get")]
        public IActionResult Get(long id)
        {
            var filename = $"estimate-{id}.pdf";
            
            var cd = new System.Net.Mime.ContentDisposition
            {
                // for example foo.bak
                FileName = filename,

                // always prompt the user for downloading, set to true if you want 
                // the browser to try to show the file inline
                Inline = false,
            };
            
            FileInfo fi = new FileInfo(Path.Combine(Startup.UserBillsDirName,filename));
           
            FileStreamResult result = null;
            var s = fi.OpenRead();
            
            result = File(s,"application/x-pdf",filename);
            
            return result;
        }
    }
}