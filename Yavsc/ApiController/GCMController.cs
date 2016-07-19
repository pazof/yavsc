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
            if (_context.GCMDevices.Any(d => d.DeviceId == declaration.DeviceId))
            {
                var alreadyRegisteredDevice = _context.GCMDevices.FirstOrDefault(d => d.DeviceId == declaration.DeviceId);
                if (alreadyRegisteredDevice.DeviceOwnerId != uid)
                {
                    return new BadRequestObjectResult(new { error = $"Device owned by someone else {declaration.DeviceId}" });
                }
                alreadyRegisteredDevice.GCMRegistrationId = declaration.GCMRegistrationId;
                alreadyRegisteredDevice.Model = declaration.Model;
                alreadyRegisteredDevice.Platform = declaration.Platform;
                alreadyRegisteredDevice.Version = declaration.Version;
                _context.Update(alreadyRegisteredDevice);
                _context.SaveChanges();
            }
            else
            {
                declaration.DeviceOwnerId = uid;
                _context.GCMDevices.Add(declaration);
                _context.SaveChanges();
            }
            return Ok();
        }
        return new BadRequestObjectResult(ModelState);
    }

}
