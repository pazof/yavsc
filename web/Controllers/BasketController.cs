using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace Yavsc.Controllers
{
	// TODO should mostly be an API Controller
    public class BasketController : Controller
    {
        public ActionResult Index()
        {
            return View ();
        }

        public ActionResult Details(int id)
        {
            return View ();
        }

        public ActionResult Create()
        {
			throw new NotImplementedException();
			// var user = Membership.GetUser ();
			// var username = (user != null)?user.UserName:Request.AnonymousID;
			// get an existing basket 
		
			//return View ();
        } 

        [HttpPost]
        public ActionResult Create(FormCollection collection)
        {
            try {
                return RedirectToAction ("Index");
            } catch {
                return View ();
            }
        }
        
        public ActionResult Edit(int id)
        {
            return View ();
        }

        [HttpPost]
        public ActionResult Edit(int id, FormCollection collection)
        {
            try {
                return RedirectToAction ("Index");
            } catch {
                return View ();
            }
        }

        public ActionResult Delete(int id)
        {
            return View ();
        }

        [HttpPost]
        public ActionResult Delete(int id, FormCollection collection)
        {
            try {
                return RedirectToAction ("Index");
            } catch {
                return View ();
            }
        }
    }
}