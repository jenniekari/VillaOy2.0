using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using VillaOy.Models;
using VillaOy.ViewModels;

namespace VillaOy.Controllers
{
    public class StatisticsController : Controller
    {
        private VillaOyEntities db = new VillaOyEntities();

        // GET: Statistics
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Sales()
        {
            string nameList;
            string salesList;
            List<SalesClass> SalesList = new List<SalesClass>();

            var salesData = from sd in db.Product_Sales_forAllTimes
                            select sd;

            foreach(Product_Sales_forAllTimes salesAllTimes in salesData)
            {
                SalesClass OneSalesRow = new SalesClass();
                OneSalesRow.Nimi = salesAllTimes.Nimi;
                OneSalesRow.ProductSales = salesAllTimes.ProductSales.ToString();
                SalesList.Add(OneSalesRow);

            }

            nameList = "'" + string.Join("','", SalesList.Select(n => n.Nimi).ToList()) + "'";
            salesList = string.Join(",", SalesList.Select(n => n.ProductSales).ToList());

            ViewBag.nimi = nameList;
            ViewBag.productSales = salesList;

            return View();
        }

        public ActionResult Sales10Best()
        {
            string nameList;
            string salesList;
            List<SalesClass> SalesList = new List<SalesClass>();

            var salesData = from sd in db.ProductSales10Best_forAllTimes
                            select sd;

            foreach (ProductSales10Best_forAllTimes sales10best in salesData)
            {
                SalesClass OneSalesRow = new SalesClass();
                OneSalesRow.Nimi = sales10best.Nimi;
                OneSalesRow.ProductSales = sales10best.ProductSales.ToString();
                SalesList.Add(OneSalesRow);

            }

            nameList = "'" + string.Join("','", SalesList.Select(n => n.Nimi).ToList()) + "'";
            salesList = string.Join(",", SalesList.Select(n => n.ProductSales).ToList());

            ViewBag.nimi = nameList;
            ViewBag.productSales = salesList;

            return View();
        }
    }
}