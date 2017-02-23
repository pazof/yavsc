
using System;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNet.Authorization;
using Microsoft.AspNet.Mvc;
using Microsoft.Extensions.Logging;
using Yavsc.Models;
using Yavsc.Models.Identity;

[Authorize, Route("~/api/gcm")]
public class GCMController : Controller
{
    ILogger _logger;
    ApplicationDbContext _context;

    public GCMController(ApplicationDbContext context,
     ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger<GCMController>();
        _context = context;
    }
    /// <summary>
    /// This is not a method supporting user creation.
    /// It only registers Google Clood Messaging id.
    /// </summary>
    /// <param name="declaration"></param>
    /// <returns></returns>
    [Authorize, HttpPost("register")]
    public IActionResult Register(
        [FromBody] GivenGoogleCloudMobileDeclaration declaration)
    {
        var uid = User.GetUserId();
        
        _logger.LogInformation($"Registering device with id:{declaration.DeviceId} for {uid}");
        if (ModelState.IsValid)
        {
            var alreadyRegisteredDevice = _context.GCMDevices.FirstOrDefault(d => d.DeviceId == declaration.DeviceId);
            var deviceAlreadyRegistered = (alreadyRegisteredDevice!=null);
            if (deviceAlreadyRegistered)
            {
                 _logger.LogInformation($"deviceAlreadyRegistered");
                // Override an exiting owner
                alreadyRegisteredDevice.DeclarationDate = DateTime.Now;
                alreadyRegisteredDevice.DeviceOwnerId = uid;
                alreadyRegisteredDevice.GCMRegistrationId = declaration.GCMRegistrationId;
                alreadyRegisteredDevice.Model = declaration.Model;
                alreadyRegisteredDevice.Platform = declaration.Platform;
                alreadyRegisteredDevice.Version = declaration.Version;
                _context.Update(alreadyRegisteredDevice);
                _context.SaveChanges(User.GetUserId());
            }
            else
            {
                 _logger.LogInformation($"new device");
                declaration.DeclarationDate = DateTime.Now;
                declaration.DeviceOwnerId = uid;
                _context.GCMDevices.Add(declaration as GoogleCloudMobileDeclaration);
                _context.SaveChanges(User.GetUserId());
            }
            var latestActivityUpdate = _context.Activities.Max(a=>a.DateModified);
            return Json(new { 
                IsAnUpdate = deviceAlreadyRegistered, 
                UpdateActivities = latestActivityUpdate > declaration.LatestActivityUpdate 
                });
        }
        return new BadRequestObjectResult(ModelState);
    }

}
