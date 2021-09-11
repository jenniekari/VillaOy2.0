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

        [HttpPost]
        public ActionResult Autherize(Logins LoginModel)
        {
            var LoggedUser = db.Logins.SingleOrDefault(x => x.UserName == LoginModel.UserName && x.PassWord == LoginModel.PassWord);
            if (LoggedUser != null)
            {
                //ViewBag.LoginMessage = "Kirjautuminen onnistui!";
                ViewBag.LoggedStatus = "In";
                Session["UserName"] = LoggedUser.UserName;
                return RedirectToAction("Index", "Home"); //Tässä määritellään mihin onnistunut kirjautuminen johtaa
            }
            else
            {
                //ViewBag.LoginMessage = "Kirjautuminen epäonnistui.";
                ViewBag.LoggedStatus = "Out";
                LoginModel.ErrorMessage = "Tuntematon käyttäjätunnus tai salasana."; //tätä ei näytetä, sillä epäonnistunut kirjautuminen viedään
                return RedirectToAction("Login", "TuotteetAdmin");
                //return View("Index", LoginModel);
            }
            /*
            using (VillaOyEntities db = new VillaOyEntities())
            {
                var userDetails = db.Logins.Where(x => x.UserName == LoginModel.UserName && x.PassWord == LoginModel.PassWord).FirstOrDefault();
                if (userDetails == null)
                {
                    LoginModel.ErrorMessage = "Väärä käyttäjä tai salasana.";
                    return View("Index", LoginModel);
                }
                else
                {
                    Session["LoginId"] = userDetails.LoginId;
                    return RedirectToAction("Index", "Home");
                }
            }*/
        }

        public ActionResult LogOut()
        {
            Session.Abandon(); //this clears all the session variables
            ViewBag.LoggedStatus = "Out";
            return RedirectToAction("Index", "Home"); //Uloskirjautumisen jälkeen pääsivulle
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
        public ActionResult Tuotteet(string currentFilter1, string searchString1)
        {
            var tuotteet = from p in db.Tuotteet
                           select p;

            if (!String.IsNullOrEmpty(searchString1))
            {
                tuotteet = tuotteet.Where(p => p.Nimi.Contains(searchString1));
            }

            return View(tuotteet);
            //return View(db.Tuotteet.ToList());
        }

        public ActionResult Tuotekuva()
        {
            return View(db.Tuotteet.ToList());
        }

    }
}