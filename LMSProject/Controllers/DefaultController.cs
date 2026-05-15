using Microsoft.AspNetCore.Mvc;

namespace LMSProject.Controllers
{
    public class DefaultController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
