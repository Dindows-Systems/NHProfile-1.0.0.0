using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.UI.WebControls;
using System.Collections.ObjectModel;

namespace NHibernate.Profile.Example.Controllers
{
    public class ProfileController : Controller
    {
        [Authorize]
        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult Index()
        {
            object timeZone = HttpContext.Profile.GetPropertyValue("TimeZone");
            ViewData["TimeZone"] = new SelectList(TimeZoneInfo.GetSystemTimeZones(), "Id", "DisplayName", timeZone);
            return View();
        }

        [Authorize]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Index(string TimeZone)
        {
            HttpContext.Profile.SetPropertyValue("TimeZone", TimeZone);
            return RedirectToAction("Index");
        }
    }
}
