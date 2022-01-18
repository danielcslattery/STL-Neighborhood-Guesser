using Microsoft.AspNetCore.Mvc;

namespace STL_Neighborhood_Guesser.Controllers
{
    public class HomeController : Controller
    {
        [Route("Neighborhoods")]
        public IActionResult Index()
        {
            return View();
        }
    }
}
