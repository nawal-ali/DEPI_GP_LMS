using LMSProject.Areas.SuperAdmin.Services;
using LMSProject.Areas.SuperAdmin.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LMSProject.Areas.SuperAdmin.Controllers
{
    [Area("SuperAdmin")]
    [Authorize(Roles = "SuperAdmin")]
    public class AnnouncementsController : Controller
    {
        private readonly SuperAdminDataService _data;
        public AnnouncementsController(SuperAdminDataService data) => _data = data;

        public IActionResult Index(string search = "", string priority = "",
                                   string status = "", string target = "", int page = 1)
        {
            ViewData["Title"] = "Announcements";
            ViewData["Breadcrumb"] = new List<(string, string?)> { ("Announcements", null) };
            var vm = _data.GetAnnouncements(search, priority, status, target, page);
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(CreateAnnouncementVM vm)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Title and content are required.";
                return RedirectToAction("Index");
            }
            _data.CreateAnnouncement(vm);
            TempData["Success"] = "Announcement published successfully.";
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            _data.DeleteAnnouncement(id);
            TempData["Success"] = "Announcement deleted.";
            return RedirectToAction("Index");
        }
    }
}