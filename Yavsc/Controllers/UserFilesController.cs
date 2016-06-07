using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Authorization;
using Microsoft.AspNet.Hosting;
using Microsoft.AspNet.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.OptionsModel;
using Microsoft.Net.Http.Headers;
using Yavsc.Models;

namespace Yavsc.Controllers
{
    [Authorize, ServiceFilter(typeof(LanguageActionFilter))]
    public class UserFilesController : Controller
    {
        private SiteSettings _siteSettings;
        IHostingEnvironment _environment;
        private IAuthorizationService _authorizationService;
        ApplicationDbContext _context;
        ILogger _logger;
        public UserFilesController(
            ApplicationDbContext context,
            IHostingEnvironment environment, IOptions<SiteSettings> siteSettings,
        IAuthorizationService authorizationService, ILoggerFactory loggerFactory)
        {
            _context = context;
            _siteSettings = siteSettings.Value;
            _environment = environment;
            _authorizationService = authorizationService;
            _logger = loggerFactory.CreateLogger<UserFilesController>();
        }

        [HttpPost, Produces("application/json")]
        public async Task<IActionResult> Create(BlogFilesPost model)
        {
            var blogEntry = _context.Blogspot.FirstOrDefault(
                be => be.Id == model.PostId);
            if (blogEntry == null)
                return new HttpNotFoundResult();
            if (!ModelState.IsValid)
                return new BadRequestObjectResult(ModelState);
            var results = new List<string>();
            var uploads = Path.Combine(_environment.WebRootPath, _siteSettings.UserFiles.DirName);
            uploads = Path.Combine(uploads, model.PostId.ToString());
            var spot = new FileSpotInfo(uploads, blogEntry);
            if (!await _authorizationService.AuthorizeAsync(User, spot, new EditRequirement()))
            {
                return new HttpStatusCodeResult(403);
            }
            if (!spot.PathInfo.Exists) spot.PathInfo.Create();
            foreach (var file in model.File)
            {
                var fileName = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"');
                var permUrl = $"~/{_siteSettings.UserFiles.DirName}/{model.PostId}/{fileName}";
                results.Add(permUrl);
                _logger.LogWarning($"Create: {model.PostId} {file.ContentDisposition}");
               /* if (file.Length > 0)
                {
                    var fileName = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"');
                    var fullName = Path.Combine(spot.PathInfo.FullName, fileName);
                    results.Add(permUrl);
                    await file.SaveAsAsync(fullName);
                } */
            }
            return Ok(results);
        }
    }

}