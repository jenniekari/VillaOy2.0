using PagedList;
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
        public ActionResult Tuotteet(string sortOrder, string currentFilter1, string searchString1, int? page, int? pagesize)
        {
            ViewBag.CurrentSort = sortOrder;
            //if-lause vb.pnsp jälkeen = Jos ensimmäinen lause on tosi ? toinen lause toteutuu : jos epätosi, niin tämä kolmas lause toteutuu
            ViewBag.ProductNameSortParm = String.IsNullOrEmpty(sortOrder) ? "productname_desc" : "";
            ViewBag.UnitPriceSortParm = sortOrder == "UnitPrice" ? "UnitPrice_desc" : "UnitPrice";

            //Hakufiltterin laitto muistiin
            if (searchString1 != null) //tarkistetaan onko käyttäjän antama arvo (esim. kirjain a tai sana villa) eri suuruinen kuin null
            {
                page = 1; //jos a-kirjainta etitään, niin vie sivulle 1 kaikki tuotteet, jossa a-kirjain
            }
            else
            {
                searchString1 = currentFilter1;
            }

            ViewBag.currentFilter1 = searchString1;

            var tuotteet = from p in db.Tuotteet
                           select p;

            if (!String.IsNullOrEmpty(searchString1)) //Jos hakufiltteri on käytössä, niin käytetään sitä ja sen lisäksi lajitellaan tulokset
            {
                switch (sortOrder)
                {
                    case "productname_desc":
                        tuotteet = tuotteet.Where(p => p.Nimi.Contains(searchString1)).OrderByDescending(p => p.Nimi);
                        break;
                    case "UnitPrice":
                        tuotteet = tuotteet.Where(p => p.Nimi.Contains(searchString1)).OrderBy(p => p.Ahinta);
                        break;
                    case "UnitPrice_desc":
                        tuotteet = tuotteet.Where(p => p.Nimi.Contains(searchString1)).OrderByDescending(p => p.Ahinta);
                        break;
                    default:
                        tuotteet = tuotteet.Where(p => p.Nimi.Contains(searchString1)).OrderBy(p => p.Nimi);
                        break;
                }
            }
            else
            {
                switch (sortOrder)
                {
                    case "productname_desc":
                        tuotteet = tuotteet.OrderByDescending(p => p.Nimi);
                        break;
                    case "UnitPrice":
                        tuotteet = tuotteet.OrderBy(p => p.Ahinta);
                        break;
                    case "UnitPrice_desc":
                        tuotteet = tuotteet.OrderByDescending(p => p.Ahinta);
                        break;
                    default:
                        tuotteet = tuotteet.OrderBy(p => p.Nimi);
                        break;
                }
            };

            //tuotteet = tuotteet.Where(p => p.Nimi.Contains(searchString1));

            int pageSize = (pagesize ?? 10); //Tämä palauttaa sivukoon taikka jos pagesize on null, niin palauttaa koon 10 riviä per sivu
            int pageNumber = (page ?? 1); //int pageNumber on sivuparametrien arvojen asetus. Tämä palauttaa sivunumeron taikka jos page on null, niin palauttaa numeron yksi
            return View(tuotteet.ToPagedList(pageNumber, pageSize));
            //return View(tuotteet);
            //return View(db.Tuotteet.ToList());
        }

        public ActionResult Tuotekuva()
        {
            return View(db.Tuotteet.ToList());
        }

    }
}