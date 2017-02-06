
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
        [FromBody] GoogleCloudMobileDeclaration declaration)
    {
        var uid = User.GetUserId();
        
        _logger.LogWarning($"Registering device with id:{declaration.DeviceId} for {uid}");
        if (ModelState.IsValid)
        {
            var alreadyRegisteredDevice = _context.GCMDevices.FirstOrDefault(d => d.DeviceId == declaration.DeviceId);
            var deviceAlreadyRegistered = (alreadyRegisteredDevice!=null);
            if (deviceAlreadyRegistered)
            {
                // Override an exiting owner
                alreadyRegisteredDevice.DeclarationDate = DateTime.Now;
                alreadyRegisteredDevice.DeviceOwnerId = uid;
                alreadyRegisteredDevice.GCMRegistrationId = declaration.GCMRegistrationId;
                alreadyRegisteredDevice.Model = declaration.Model;
                alreadyRegisteredDevice.Platform = declaration.Platform;
                alreadyRegisteredDevice.Version = declaration.Version;
                _context.Update(alreadyRegisteredDevice);
                _context.SaveChanges();
            }
            else
            {
                declaration.DeclarationDate = DateTime.Now;
                declaration.DeviceOwnerId = uid;
                _context.GCMDevices.Add(declaration);
                _context.SaveChanges();
            }
            if (declaration.LatestActivityUpdate > default(DateTime))
            {
                var latestActivityUpdate = _context.Activities.Aggregate(
                    (a,b)=>a.DateModified>b.DateModified?a:b
                ).DateModified;
                return Json(new { 
                    IsAnUpdate = deviceAlreadyRegistered, 
                    UpdateActivities = latestActivityUpdate>declaration.LatestActivityUpdate?true:false 
                    });
            }
            return Json(new { IsAnUpdate = deviceAlreadyRegistered });
        }
        return new BadRequestObjectResult(ModelState);
    }

}
