using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace OPKODABbl.Controllers
{
    public class PageController : Controller
    {
        public IActionResult Roster()
        {
            return View();
        }
    }
}
