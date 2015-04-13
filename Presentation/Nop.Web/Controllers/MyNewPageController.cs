using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Nop.Web.Controllers
{
    public class MyNewPageController : Controller
    {
        // GET: MyNewPage
        public ActionResult Index()
        {
            return View();
        }
    }
}