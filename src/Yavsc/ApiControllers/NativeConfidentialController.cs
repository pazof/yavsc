
using System;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNet.Authorization;
using Microsoft.AspNet.Mvc;
using Microsoft.Extensions.Logging;
using Yavsc.Models;
using Yavsc.Models.Identity;

[Authorize, Route("~/api/gcm")]
public class NativeConfidentialController : Controller
{
    readonly ILogger _logger;
    readonly ApplicationDbContext _context;

    public NativeConfidentialController(ApplicationDbContext context,
     ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger<NativeConfidentialController>();
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
        [FromBody] DeviceDeclaration declaration)
    {
      var uid = User.GetUserId();

      if (!ModelState.IsValid)
      {
        _logger.LogError("Invalid model for GCMD");
        return new BadRequestObjectResult(ModelState);
      }
      declaration.LatestActivityUpdate = DateTime.Now;

      _logger.LogInformation($"Registering device with id:{declaration.DeviceId} for {uid}");
      var alreadyRegisteredDevice = _context.DeviceDeclaration.FirstOrDefault(d => d.DeviceId == declaration.DeviceId);
      var deviceAlreadyRegistered = (alreadyRegisteredDevice!=null);
      if (deviceAlreadyRegistered)
      {
        _logger.LogInformation($"deviceAlreadyRegistered");
        // Override an exiting owner
        alreadyRegisteredDevice.DeviceOwnerId = uid;
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
        _context.DeviceDeclaration.Add(declaration as DeviceDeclaration);
        _context.SaveChanges(User.GetUserId());
      }
      var latestActivityUpdate = _context.Activities.Max(a=>a.DateModified);
      return Json(new { 
          IsAnUpdate = deviceAlreadyRegistered, 
          UpdateActivities = (latestActivityUpdate != declaration.LatestActivityUpdate)
          });
    }

}
