using System.Linq;
using Microsoft.AspNet.Authorization;
using Microsoft.AspNet.Mvc;
using Microsoft.Extensions.Logging;
using Yavsc.Models;

public class GCMController : Controller {
    ILogger _logger;
    ApplicationDbContext _context;
    
    public GCMController (ApplicationDbContext context,
     ILoggerFactory loggerFactory) 
    {
        _logger = loggerFactory.CreateLogger<GCMController>();
        _context = context;
    }
    
    [Authorize]
    public void Register (GoogleCloudMobileDeclaration declaration)
    {
        if (_context.GCMDevices.Any(d => d.RegistrationId == declaration.RegistrationId))
        { 
            var alreadyRegisteredDevice = _context.GCMDevices.FirstOrDefault(d => d.RegistrationId == declaration.RegistrationId);
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
    }

}