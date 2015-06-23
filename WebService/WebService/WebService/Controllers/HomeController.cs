using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace WebService.Controllers
{
    public class HomeController : Controller
    {
        //
        // GET: /Home/

        public ActionResult Index()
        {
            return View();
        }


        public string SendImage(FormCollection form)
        {
            string Image = form["Image"];
            Service.WebService1SoapClient client = new Service.WebService1SoapClient();
            int x = client.HelloWorld(2, 2);
            return x.ToString();
        }
    }
}
