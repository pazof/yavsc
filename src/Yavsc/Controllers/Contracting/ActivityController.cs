using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNet.Authorization;
using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Mvc.Rendering;
using Microsoft.Data.Entity;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace Yavsc.Controllers
{
    using System.Security.Claims;
    using Models;
    using Models.Workflow;

    [Authorize("AdministratorOnly")]
    public class ActivityController : Controller
    {
        private readonly ApplicationDbContext _context;
        readonly IStringLocalizer<Yavsc.YavscLocalisation> SR;
        readonly ILogger logger;

        public ActivityController(ApplicationDbContext context, 
        IStringLocalizer<Yavsc.YavscLocalisation> SR,
        ILoggerFactory loggerFactory)
        {
            _context = context;
            this.SR = SR;
            logger=loggerFactory.CreateLogger<ActivityController>();
        }

        // GET: Activity
        public IActionResult Index()
        {
            SetSettingClasseInfo();
            return View(_context.Activities.Include(a=>a.Parent).ToList());
        }

        private void SetSettingClasseInfo(string currentCode = null)
        {
            var items = Startup.ProfileTypes.Select(
                pt => new SelectListItem
                {
                    Text = SR[pt.FullName],
                    Value = pt.FullName,
                    Selected = currentCode == pt.FullName
                }).ToList();
            items.Add(new SelectListItem { Text = SR[Constants.NoneCode], Value = Constants.NoneCode, Selected = currentCode == null});
            ViewBag.SettingsClassName = items;
        }

        private List<SelectListItem> GetEligibleParent(string code)
        {
            // eligibles are those
            // who are not in descendants

            //
            var acts = _context.Activities.Where(
                a => a.Code != code
            ).Select(a => new SelectListItem
            {
                Text = a.Name,
                Value = a.Code
            }).ToList();
            var nullItem = new SelectListItem { Text = SR[Constants.NoneCode], Value = Constants.NoneCode };
            acts.Add(nullItem);
            if (code == null) return acts;
            var existing = _context.Activities.Include(a => a.Children).FirstOrDefault(a => a.Code == code);
            if (existing == null) return acts;
            var pi = acts.FirstOrDefault(i => i.Value == existing.ParentCode);
            if (pi!=null) pi.Selected = true; 
            else nullItem.Selected = true;
            RecFilterChild(acts, existing); 
            return acts;
        }

        /// <summary>
        /// Filters a activity selection list
        /// in order to exculde any descendant 
        /// from the eligible list at the <c>Parent</c> property.
        /// WARN! results in a infinite loop when
        /// data is corrupted and there is a circularity
        /// in the activity hierarchy graph (Parent/Children)
        /// </summary>
        /// <param name="list"></param>
        /// <param name="activity"></param>
        private static void RecFilterChild(List<SelectListItem> list, Activity activity)
        {
            if (activity == null) return;
            if (activity.Children == null) return;
            if (list.Count == 0) return;
            foreach (var child in activity.Children)
            {
                RecFilterChild(list, child);
                var rem = list.FirstOrDefault(i => i.Value == child.Code);
                if (rem != null) list.Remove(rem);
            }
        }

      


        // GET: Activity/Details/5
        public IActionResult Details(string id)
        {
            if (id == null)
            {
                return HttpNotFound();
            }

            Activity activity = _context.Activities.Single(m => m.Code == id);
            if (activity == null)
            {
                return HttpNotFound();
            }

            return View(activity);
        }

        // GET: Activity/Create
        public IActionResult Create()
        {
            SetSettingClasseInfo();
            ViewBag.ParentCode = GetEligibleParent(null);
            return View();
        }

        // POST: Activity/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Activity activity)
        {
            if (activity.ParentCode==Constants.NoneCode)
                activity.ParentCode=null;
            if (activity.SettingsClassName==Constants.NoneCode)
                activity.SettingsClassName=null;

            if (ModelState.IsValid)
            {
                _context.Activities.Add(activity);
                _context.SaveChanges(User.GetUserId());
                return RedirectToAction("Index");
            }
            SetSettingClasseInfo();
            return View(activity);
        }

        // GET: Activity/Edit/5
        public IActionResult Edit(string id)
        {
            if (id == null)
            {
                return HttpNotFound();
            }

            Activity activity = _context.Activities.Single(m => m.Code == id);
            if (activity == null)
            {
                return HttpNotFound();
            }
            ViewBag.ParentCode = GetEligibleParent(id);
            SetSettingClasseInfo();
            return View(activity);
        }

        // POST: Activity/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Activity activity)
        {
            if (activity.ParentCode==Constants.NoneCode)
                activity.ParentCode=null;
            if (activity.SettingsClassName==Constants.NoneCode)
                activity.SettingsClassName=null;
            if (ModelState.IsValid)
            {
                _context.Update(activity);
                _context.SaveChanges(User.GetUserId());
                return RedirectToAction("Index");
            }
            return View(activity);
        }

        // GET: Activity/Delete/5
        [ActionName("Delete")]
        public IActionResult Delete(string id)
        {
            if (id == null)
            {
                return HttpNotFound();
            }

            Activity activity = _context.Activities.Single(m => m.Code == id);
            if (activity == null)
            {
                return HttpNotFound();
            }

            return View(activity);
        }

        // POST: Activity/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(string id)
        {
            Activity activity = _context.Activities.Single(m => m.Code == id);
            _context.Activities.Remove(activity);
            _context.SaveChanges(User.GetUserId());
            return RedirectToAction("Index");
        }
    }
}
