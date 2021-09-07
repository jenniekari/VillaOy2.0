using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using VillaOy.Models;

namespace VillaOy.Controllers
{
    public class HomeController : Controller
    {
        private VillaOyEntities db = new VillaOyEntities();

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Meistä yrityksenä";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Yhteystiedot";

            return View();
        }
        public ActionResult Tuotteet()
        {
            return View(db.Tuotteet.ToList());
        }

        public ActionResult Tuotekuva()
        {
            return View(db.Tuotteet.ToList());
        }

    }
}