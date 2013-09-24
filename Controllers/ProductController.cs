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
               
            List<FoundItem> fitems= fp.process();
            ViewBag.pcount = ((fp.getTotalResults())/13)+1;
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

       

       

       
    }
}
