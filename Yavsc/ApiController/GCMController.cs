using System.Linq;
using System.Security.Claims;
using Microsoft.AspNet.Authorization;
using Microsoft.AspNet.Mvc;
using Microsoft.Extensions.Logging;
using Yavsc.Models;

[Authorize,Route("~/api/gcm")]
public class GCMController : Controller {
    ILogger _logger;
    ApplicationDbContext _context;
    
    public GCMController (ApplicationDbContext context,
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
    public IActionResult Register (GoogleCloudMobileDeclaration declaration)
    {
        if (declaration.DeviceOwnerId!=null)
            if (User.GetUserId() != declaration.DeviceOwnerId)
                return new BadRequestObjectResult(
                    new { error = "you're not allowed to register for another user" } 
                );
        declaration.DeviceOwnerId = User.GetUserId();
        if (_context.GCMDevices.Any(d => d.DeviceId == declaration.DeviceId))
        { 
            var alreadyRegisteredDevice = _context.GCMDevices.FirstOrDefault(d => d.DeviceId == declaration.DeviceId);
            // Assert alreadyRegisteredDevice != null
            if (alreadyRegisteredDevice != declaration) {
                _context.GCMDevices.Update(declaration);
                _context.SaveChanges();
            } // else nothing to do.
        }
        else 
        {
            _context.GCMDevices.Add(declaration);
            _context.SaveChanges();
        }
        return Ok();
    }

}