using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;


namespace stitalizator01.Controllers
{
    
    public class HomeController : Controller
    {
        private static readonly log4net.ILog logH = log4net.LogManager.GetLogger("HomeController.cs");
        [AllowAnonymous]
        public ActionResult Index()
        {
            if (User.Identity.IsAuthenticated)
            {
                logH.Info("User " + User.Identity.Name + " has opened Index page");
                return View();
            }
            else
            {
                return RedirectToAction("BlockScreen");
            }
        }

        [AllowAnonymous]
        public ActionResult BlockScreen()
        {
            ViewBag.ReturnUrl = "";
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}