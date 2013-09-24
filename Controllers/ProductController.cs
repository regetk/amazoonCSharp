using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Mvc;
using Amazoon.Models;
using System.Web.Helpers;
using System.Web;
using System.Xml.Linq;

namespace Amazoon.Controllers
{
    public class ProductController : Controller
    {
        
        
        public ActionResult books(string fWord="",string sCurr="EUR",string page="1")
        {
            ViewBag.fword = fWord;
            ViewBag.scurr = sCurr;
            FindProd fp = new FindProd(fWord, page);
            string stp;
            List<FoundItem> fitems= fp.process(1,1,out stp);
            ViewBag.pcount = Convert.ToInt32(stp);
            return View(fitems);
        }

        public string CurrConv(long? iexp, string toCurr="EUR") {
          String url = "http://www.webservicex.net/CurrencyConvertor.asmx/ConversionRate?";
          String link = url + "FromCurrency=EUR&ToCurrency=" + toCurr;
          XDocument xdoc = XDocument.Load(link);
          string nsp = @"{http://www.webserviceX.NET/}";
          XElement xe = xdoc.Element(nsp+"double");
          return xe.Value;
        }

        public JsonResult lazyPaging(string page) {
            FindProd fp = new FindProd("rocket", page);
            string stp;
            List<FoundItem> fitems = fp.process(1, 1, out stp);
            WebGrid grid = new WebGrid(fitems);
            IHtmlString htmlStr = grid.GetHtml(tableStyle: "webGrid",
            headerStyle: "header", htmlAttributes: new { id = "fitems" });
            return Json(new { items=htmlStr.ToHtmlString(), pCount=stp
                            },JsonRequestBehavior.AllowGet
                );


        }

       

       
    }
}
