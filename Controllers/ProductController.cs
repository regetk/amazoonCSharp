using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Mvc;
using Amazoon.Models;

namespace Amazoon.Controllers
{
    public class ProductController : Controller
    {
        
        
        public ActionResult page()
        {
            FindProd fp = new FindProd("rocket", "1");
            string stp;
            List<FoundItem> fitems= fp.process(1,1,out stp);
            return View(fitems);
        }

       

       
    }
}
