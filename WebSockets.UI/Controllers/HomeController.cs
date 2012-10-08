using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace WebSockets.UI.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Draw()
        {
            ViewBag.Title = "Draw";
            return View();
        }

        public ActionResult Chat()
        {
            ViewBag.Title = "Title";
            return View();
        }
    }
}
