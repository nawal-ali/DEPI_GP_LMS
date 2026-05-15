using LMSProject.Areas.SuperAdmin.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LMSProject.Areas.SuperAdmin.Controllers
{
    [Area("SuperAdmin")]
    [Authorize(Roles = "SuperAdmin")]
    public class StatisticsController : Controller
    {
        private readonly SuperAdminDataService _data;
        public StatisticsController(SuperAdminDataService data) => _data = data;

        public IActionResult Index()
        {
            ViewData["Title"] = "System Statistics";
            ViewData["Breadcrumb"] = new List<(string, string?)> { ("Analytics", null), ("System Statistics", null) };
            return View(_data.GetStatistics());
        }

        public IActionResult Users()
        {
            ViewData["Title"] = "User Analytics";
            ViewData["Breadcrumb"] = new List<(string, string?)> { ("Analytics", null), ("User Analytics", null) };
            return View(_data.GetStatistics());
        }

        public IActionResult Courses()
        {
            ViewData["Title"] = "Course Analytics";
            ViewData["Breadcrumb"] = new List<(string, string?)> { ("Analytics", null), ("Course Analytics", null) };
            return View(_data.GetStatistics());
        }
    }
}