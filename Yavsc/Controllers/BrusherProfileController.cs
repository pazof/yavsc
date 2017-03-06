using System.Threading.Tasks;
using Microsoft.AspNet.Mvc;
using System.Security.Claims;
using Microsoft.Data.Entity;
using Yavsc.Models;
using Yavsc.Models.Haircut;
using Microsoft.AspNet.Authorization;
using Yavsc.Controllers.Generic;

namespace Yavsc.Controllers
{
    [Authorize(Roles="Performer")]
    public class BrusherProfileController : SettingsController<BrusherProfile>
    {

        public BrusherProfileController(ApplicationDbContext context) : base(context)
        {
        }
        override public async Task<IActionResult> Edit(BrusherProfile brusherProfile)
        {
            if (string.IsNullOrEmpty(brusherProfile.UserId))
            {
                // a creation
                brusherProfile.UserId = User.GetUserId();
                if (ModelState.IsValid)
                {
                    _context.BrusherProfile.Add(brusherProfile);
                    await _context.SaveChangesAsync();
                    return RedirectToAction("Index");
                }
            } 
            else if (ModelState.IsValid)
            {
                _context.Update(brusherProfile);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(brusherProfile);
        }

        // POST: BrusherProfile/Delete/5
        override public async Task<IActionResult> DeleteConfirmed(string id)
        {
            BrusherProfile brusherProfile = await _context.BrusherProfile.SingleAsync(m => m.UserId == id);
            _context.BrusherProfile.Remove(brusherProfile);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }
    }
}
