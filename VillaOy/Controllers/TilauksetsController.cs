using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using VillaOy.Models;
using PagedList;
using VillaOy.ViewModels;


namespace VillaOy.Controllers
{
    public class TilauksetsController : Controller
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
                return RedirectToAction("Index", "Tilauksets"); //Tässä määritellään mihin onnistunut kirjautuminen johtaa
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

        // GET: Tilauksets
        public ActionResult Index(string sortOrder, string currentFilter1, string searchString1, string AsiakasID, string Postinumero, string currentAsiakkaatCategory, int? page, int? pagesize)
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
                ViewBag.CustomerNameSortParm = String.IsNullOrEmpty(sortOrder) ? "customername_desc" : "";
                ViewBag.ZipCodeSortParm = sortOrder == "ZipCode" ? "ZipCode_desc" : "ZipCode";

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

                //tämä on sama kuin yllä, mutta kategorioille. Jos arvoa ei ole annettu tai on, niin sivu on 1, eli filtteri on juuri astunut voimaan.
                if((AsiakasID != null) && (AsiakasID != "0"))
                {
                    page = 1;
                }
                else
                {
                    AsiakasID = currentAsiakkaatCategory;
                }
                var tilaukset = from p in db.Tilaukset
                                select p;

                if (!String.IsNullOrEmpty(searchString1)) //Jos hakufiltteri on käytössä, niin käytetään sitä ja sen lisäksi lajitellaan tulokset
                {
                    switch (sortOrder)
                    {
                        case "customername_desc":
                            tilaukset = tilaukset.Where(p => p.Asiakkaat.Nimi.Contains(searchString1)).OrderByDescending(p => p.Asiakkaat.Nimi);
                            break;
                        case "ZipCode":
                            tilaukset = tilaukset.Where(p => p.Postitoimipaikat.Postitoimipaikka.Contains(searchString1)).OrderBy(p => p.Postitoimipaikat.Postitoimipaikka);
                            break;
                        case "ZipCode_desc":
                            tilaukset = tilaukset.Where(p => p.Postitoimipaikat.Postitoimipaikka.Contains(searchString1)).OrderByDescending(p => p.Postitoimipaikat);
                            break;
                        default:
                            tilaukset = tilaukset.Where(p => p.Asiakkaat.Nimi.Contains(searchString1)).OrderBy(p => p.Asiakkaat.Nimi);
                            break;
                    }
                }
                else if (!String.IsNullOrEmpty(AsiakasID) && (AsiakasID != "0"))
                {
                    int para = int.Parse(AsiakasID);
                    switch (sortOrder)
                    {
                        case "customername_desc":
                            tilaukset = tilaukset.Where(p => p.TilausID == para).OrderByDescending(p => p.AsiakasID);
                            break;
                        case "ZipCode":
                            tilaukset = tilaukset.Where(p => p.TilausID == para).OrderBy(p => p.Postitoimipaikat.Postitoimipaikka);
                            break;
                        case "ZipCode_desc":
                            tilaukset = tilaukset.Where(p => p.TilausID == para).OrderByDescending(p => p.Postitoimipaikat.Postitoimipaikka);
                            break;
                        default:
                            tilaukset = tilaukset.Where(p => p.TilausID == para).OrderBy(p => p.AsiakasID);
                            break;
                    }
                }
                else //Tässä hakufiltteri EI OLE käytössä, joten lajitellaan koko tulosjoukko ilman suodatuksia
                {
                    switch (sortOrder)
                    {
                        case "customername_desc":
                            tilaukset = tilaukset.OrderByDescending(p => p.Asiakkaat.Nimi);
                            break;
                        case "ZipCode":
                            tilaukset = tilaukset.OrderBy(p => p.Postitoimipaikat.Postitoimipaikka);
                            break;
                        case "ZipCode_desc":
                            tilaukset = tilaukset.OrderByDescending(p => p.Postitoimipaikat.Postitoimipaikka);
                            break;
                        default:
                            tilaukset = tilaukset.OrderBy(p => p.Asiakkaat.Nimi);
                            break;
                    }
                };
                
                List<Asiakkaat> lstAsiakkaat = new List<Asiakkaat>();
                
                var asiakkaatList = from cat in db.Asiakkaat
                                    select cat;

                Asiakkaat tyhjaAsiakas = new Asiakkaat();
                tyhjaAsiakas.Nimi = "";
                tyhjaAsiakas.AsiakasID = 0;
                lstAsiakkaat.Add(tyhjaAsiakas);
                
                foreach(Asiakkaat asiakas in asiakkaatList)
                {
                    Asiakkaat yksiAsiakas = new Asiakkaat();
                    yksiAsiakas.Nimi = asiakas.Nimi;
                    yksiAsiakas.AsiakasID = asiakas.AsiakasID;
                    //yksiTilaus.CategoryIDCategoryName = tilaus.TilausID.ToString() + " - " + tilaus.AsiakasID.ToString();
                    lstAsiakkaat.Add(yksiAsiakas);
                }
                ViewBag.AsiakasID = new SelectList(lstAsiakkaat, "AsiakasID", "Nimi", AsiakasID);

                /*
                //Postitoimipaikan kautta hakeminen

                List<Postitoimipaikat> lstPostinumerot = new List<Postitoimipaikat>();

                var postList = from cat in db.Postitoimipaikat
                                    select cat;

                Postitoimipaikat tyhjaPost = new Postitoimipaikat();
                tyhjaPost.Postinumero = "";
                //tyhjaPost.Postinumero = 0;
                lstPostinumerot.Add(tyhjaPost);

                foreach (Postitoimipaikat posti in postList)
                {
                    Postitoimipaikat postiNumero = new Postitoimipaikat();
                    postiNumero.Postinumero = postiNumero.Postinumero;
                    //postiNumero.AsiakasID = asiakas.AsiakasID;
                    //yksiTilaus.CategoryIDCategoryName = tilaus.TilausID.ToString() + " - " + tilaus.AsiakasID.ToString();
                    lstPostinumerot.Add(postiNumero);
                }
                ViewBag.Postinumero = new SelectList(lstPostinumerot, "Postinumero", Postinumero);*/

                int pageSize = (pagesize ?? 10); //Tämä palauttaa sivukoon taikka jos pagesize on null, niin palauttaa koon 10 riviä per sivu
                int pageNumber = (page ?? 1); //int pageNumber on sivuparametrien arvojen asetus. Tämä palauttaa sivunumeron taikka jos page on null, niin palauttaa numeron yksi
                return View(tilaukset.ToPagedList(pageNumber, pageSize));

                //var tilaukset = db.Tilaukset.Include(t => t.Asiakkaat).Include(t => t.Postitoimipaikat);
                //return View(tilaukset.ToList());

                /*List<Tuotteet> model = db.Tuotteet.ToList();
                db.Dispose();

                return View(model);*/
            }
            /*
            var tilaukset = db.Tilaukset.Include(t => t.Asiakkaat).Include(t => t.Postitoimipaikat);
            return View(tilaukset.ToList());*/
        }

        // GET: Tilauksets/Details/5
        public ActionResult Details(int? id)
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
                Tilaukset tilaukset = db.Tilaukset.Find(id);
                if (tilaukset == null)
                {
                    return HttpNotFound();
                }
                return View(tilaukset);
            }
        }

        // GET: Tilauksets/Create
        public ActionResult Create()
        {
            if (Session["UserName"] == null)
            {
                return RedirectToAction("Login", "TuotteetAdmin");
            }
            else
            {
                ViewBag.LoggedStatus = "In";
                ViewBag.AsiakasID = new SelectList(db.Asiakkaat, "AsiakasID", "Nimi");
                ViewBag.Postinumero = new SelectList(db.Postitoimipaikat, "Postinumero", "Postitoimipaikka");
                return View();
            }
        }

        // POST: Tilauksets/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "TilausID,AsiakasID,Toimitusosoite,Postinumero,Tilauspvm,Toimituspvm")] Tilaukset tilaukset)
        {
            if (ModelState.IsValid)
            {
                db.Tilaukset.Add(tilaukset);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.AsiakasID = new SelectList(db.Asiakkaat, "AsiakasID", "Nimi", tilaukset.AsiakasID);
            ViewBag.Postinumero = new SelectList(db.Postitoimipaikat, "Postinumero", "Postitoimipaikka", tilaukset.Postinumero);
            return View(tilaukset);
        }

        // GET: Tilauksets/Edit/5
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
                Tilaukset tilaukset = db.Tilaukset.Find(id);
                if (tilaukset == null)
                {
                    return HttpNotFound();
                }
                ViewBag.AsiakasID = new SelectList(db.Asiakkaat, "AsiakasID", "Nimi", tilaukset.AsiakasID);
                ViewBag.Postinumero = new SelectList(db.Postitoimipaikat, "Postinumero", "Postitoimipaikka", tilaukset.Postinumero);
                return View(tilaukset);
            }
        }

        // POST: Tilauksets/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "TilausID,AsiakasID,Toimitusosoite,Postinumero,Tilauspvm,Toimituspvm")] Tilaukset tilaukset)
        {
            if (ModelState.IsValid)
            {
                db.Entry(tilaukset).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.AsiakasID = new SelectList(db.Asiakkaat, "AsiakasID", "Nimi", tilaukset.AsiakasID);
            ViewBag.Postinumero = new SelectList(db.Postitoimipaikat, "Postinumero", "Postitoimipaikka", tilaukset.Postinumero);
            return View(tilaukset);
        }

        public ActionResult _ModalEdit(int? id)
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
                Tilaukset tilaukset = db.Tilaukset.Find(id);
                if (tilaukset == null)
                {
                    return HttpNotFound();
                }
                ViewBag.AsiakasID = new SelectList(db.Asiakkaat, "AsiakasID", "Nimi", tilaukset.AsiakasID);
                ViewBag.Postinumero = new SelectList(db.Postitoimipaikat, "Postinumero", "Postitoimipaikka", tilaukset.Postinumero);
                return PartialView("_ModalEdit", tilaukset);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult _ModalEdit([Bind(Include = "TilausID,AsiakasID,Toimitusosoite,Postinumero,Tilauspvm,Toimituspvm")] Tilaukset tilaukset)
        {
            if (ModelState.IsValid)
            {
                db.Entry(tilaukset).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.AsiakasID = new SelectList(db.Asiakkaat, "AsiakasID", "Nimi", tilaukset.AsiakasID);
            ViewBag.Postinumero = new SelectList(db.Postitoimipaikat, "Postinumero", "Postitoimipaikka", tilaukset.Postinumero);
            return PartialView("_ModalEdit", tilaukset);
        }

        // GET: Tilauksets/Delete/5
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
                Tilaukset tilaukset = db.Tilaukset.Find(id);
                if (tilaukset == null)
                {
                    return HttpNotFound();
                }
                return View(tilaukset);
            }
        }

        // POST: Tilauksets/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Tilaukset tilaukset = db.Tilaukset.Find(id);
            db.Tilaukset.Remove(tilaukset);
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

        public ActionResult Ordersummary()
        {
            var orderSummary = from or in db.Tilaukset
                               join ti in db.Tilausrivit on or.TilausID equals ti.TilausID
                               //where-lause
                               //orderby-lause
                               select new OrderSummaryData
                               {
                                   TilausID = or.TilausID,
                                   AsiakasID = or.AsiakasID,
                                   Toimitusosoite = or.Toimitusosoite,
                                   Postinumero = or.Postinumero,
                                   Tilauspvm = (DateTime)or.Tilauspvm,
                                   Toimituspvm = (DateTime)or.Toimituspvm,
                                   TilausriviID = ti.TilausriviID,
                                   TuoteID = ti.TuoteID,
                                   Maara = ti.Maara,
                                   Ahinta = (float)ti.Ahinta,
                               };
            return View(orderSummary);
        }

        public ActionResult TilausOtsikot()
        {
            var orders = db.Tilaukset.Include(o => o.Asiakkaat).Include(o => o.Tilausrivit);
            return View(orders.ToList());
        }

        public ActionResult _Tilausrivit(int? tilausid)
        {
            if ((tilausid == null || (tilausid == 0)))
            {
                return HttpNotFound();
            }
            else
            {
                var orderRowsList = from or in db.Tilaukset
                                   join ti in db.Tilausrivit on or.TilausID equals ti.TilausID
                                   where or.TilausID == tilausid
                                   //orderby-lause
                                   select new OrderRows
                                   {
                                       TilausriviID = ti.TilausriviID,
                                       TuoteID = ti.TuoteID,
                                       Toimitusosoite = or.Toimitusosoite,
                                       Postinumero = or.Postinumero,
                                       Maara = ti.Maara,
                                       Ahinta = ti.Ahinta,
                                   };
                return PartialView(orderRowsList);
            }
        }
    }
}
