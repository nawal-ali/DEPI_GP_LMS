using LMSProject.Areas.SuperAdmin.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LMSProject.Areas.SuperAdmin.Controllers
{
    [Area("SuperAdmin")]
    [Authorize(Roles = "SuperAdmin")]
    public class HomeController : Controller
    {
        private readonly SuperAdminDataService _data;
        public HomeController(SuperAdminDataService data) => _data = data;

        public IActionResult Index()
        {
            ViewData["Title"] = "Dashboard";
            var vm = _data.GetDashboard();
            return View(vm);
        }

        public IActionResult Logout()
        {
            // Sign out handled by AccountController; redirect there
            return RedirectToAction("Logout", "Account", new { area = "" });
        }
    }
}