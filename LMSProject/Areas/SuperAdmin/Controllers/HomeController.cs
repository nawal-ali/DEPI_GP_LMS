using LMSProject.Areas.SuperAdmin.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MLSEF;

namespace LMSProject.Areas.SuperAdmin.Controllers
{
    [Area("SuperAdmin")]
    [Authorize(Roles = "SuperAdmin")]
    public class HomeController : Controller
    {
        private readonly SuperAdminDataService _data;
        private readonly AppDbContext _db;

        public HomeController(SuperAdminDataService data, AppDbContext db)
        { _data = data; _db = db; }

        public async Task<IActionResult> Index()
        {
            ViewData["Title"] = "Dashboard";
            ViewBag.UnreadContactForms = await _db.ContactForms.CountAsync(f => !f.IsRead);
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