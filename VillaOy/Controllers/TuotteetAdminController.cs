using PagedList;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using VillaOy.Models;

namespace VillaOy.Controllers
{
    public class TuotteetAdminController : Controller
    {
        VillaOyEntities db = new VillaOyEntities();

        public ActionResult Login()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Authorize(Logins LoginModel)
        {
            //Haetaan käyttäjän/Loginin tiedot annetuilla tunnustiedoilla tietokannasta LINQ -kyselyllä
            var LoggedUser = db.Logins.SingleOrDefault(x => x.UserName == LoginModel.UserName && x.PassWord == LoginModel.PassWord);
            if (LoggedUser != null)
            {
                ViewBag.LoginMessage = "Kirjautuminen onnistui!";
                ViewBag.LoggedStatus = "In";
                Session["UserName"] = LoggedUser.UserName;
                return RedirectToAction("Index", "TuotteetAdmin"); //Tässä määritellään mihin onnistunut kirjautuminen johtaa
            }
            else
            {
                ViewBag.LoginMessage = "Kirjautuminen epäonnistui.";
                ViewBag.LoggedStatus = "Out";
                LoginModel.ErrorMessage = "Tuntematon käyttäjätunnus tai salasana.";
                return View("Login", LoginModel);
            }
        }
        public ActionResult LogOut()
        {
            Session.Abandon();
            ViewBag.LoggedStatus = "Out";
            return RedirectToAction("Index", "Home"); //Uloskirjautumisen jälkeen pääsivulle
        }

        // GET: TuotteetAdmin
        public ActionResult Index(string sortOrder, string currentFilter1, string searchString1, int? page, int? pagesize)
        {
            if (Session["UserName"] == null)
            {
                return RedirectToAction("Login", "TuotteetAdmin");
            }
            else
            {
                ViewBag.LoggedStatus = "In";

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

                
                //List<Tuotteet> model = db.Tuotteet.ToList();
                //db.Dispose();

                //return View(model);
            }
        }

        // GET: TuotteetAdmin/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Tuotteet tuotteet = db.Tuotteet.Find(id);
            if (tuotteet == null)
            {
                return HttpNotFound();
            }
            return View(tuotteet);
        }

        // GET: TuotteetAdmin/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: TuotteetAdmin/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "TuoteID,Nimi,Ahinta,Kuva")] Tuotteet tuotteet)
        {
            if (Session["UserName"] == null)
            {
                return RedirectToAction("Login", "TuotteetAdmin");
            }
            else
            {
                ViewBag.LoggedStatus = "In";
                if (ModelState.IsValid)
                {
                    db.Tuotteet.Add(tuotteet);
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }

                return View(tuotteet);
            }
        }

        // GET: TuotteetAdmin/Edit/5
        public ActionResult Edit(int? id)
        {
            if (Session["UserName"] == null)
            {
                return RedirectToAction("Login", "TuotteetAdmin");
            }
            else
            {
                ViewBag.LoggedStatus = "In";
                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                Tuotteet tuotteet = db.Tuotteet.Find(id);
                if (tuotteet == null)
                {
                    return HttpNotFound();
                }
                return View(tuotteet);
            }
        }

        // POST: TuotteetAdmin/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "TuoteID,Nimi,Ahinta,Kuva")] Tuotteet tuotteet)
        {
            if (ModelState.IsValid)
            {
                db.Entry(tuotteet).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(tuotteet);
        }

        // GET: TuotteetAdmin/Delete/5
        public ActionResult Delete(int? id)
        {
            if (Session["UserName"] == null)
            {
                return RedirectToAction("Login", "TuotteetAdmin");
            }
            else
            {
                ViewBag.LoggedStatus = "In";
                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                Tuotteet tuotteet = db.Tuotteet.Find(id);
                if (tuotteet == null)
                {
                    return HttpNotFound();
                }
                return View(tuotteet);
            }
        }

        // POST: TuotteetAdmin/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Tuotteet tuotteet = db.Tuotteet.Find(id);
            db.Tuotteet.Remove(tuotteet);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
