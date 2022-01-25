using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace STL_Neighborhood_Guesser.Controllers
{
    public class HomeController : Controller
    {

        [Route("Neighborhoods")]
        public IActionResult Index()
        {
            ViewBag.LoggedIn = User.Identity.IsAuthenticated;

            return View();
        }
    }
}
