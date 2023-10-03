using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Netcore.Chat.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        #region Header - Menu
        public IActionResult Header()
        {
            return PartialView();
        }

        public IActionResult Menu()
        {
            return PartialView();
        }
        #endregion

        #region Login
        public IActionResult Login()
        {
            return PartialView();
        }

        public IActionResult FormLogin()
        {
            return PartialView();
        }
        #endregion
    }
}