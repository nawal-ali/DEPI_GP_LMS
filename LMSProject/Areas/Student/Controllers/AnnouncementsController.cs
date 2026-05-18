using LMSProject.Areas.Student.ViewModels;
using LMSProject.Controllers;
using LMSProject.Services;
using LMSProject.ViewModels.Tickets;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MLSCore.IdentityModel;
using MLSCore.Models;
using MLSEF;

namespace LMSProject.Areas.Student.Controllers
{
    [Area("Student")]
    [Authorize(Roles = "Student")]
    public class AnnouncementsController : Controller
    {
        private readonly AppDbContext _db;
        public AnnouncementsController(AppDbContext db) => _db = db;

        public async Task<IActionResult> Index(string search = "", string priority = "", int page = 1)
        {
            ViewData["Title"] = "Announcements";
            int pageSize = 8;

            var query = _db.Announcements
                .Where(a => a.CurrentState == 1 && a.IsActive
                         && (a.ExpiryDate == null || a.ExpiryDate > DateTime.Now)
                         && (a.TargetAudience == "All" || a.TargetAudience == "Students"))
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search)) query = query.Where(a => a.Title.Contains(search) || a.Content.Contains(search));
            if (!string.IsNullOrWhiteSpace(priority)) query = query.Where(a => a.Priority == priority);

            var all = await query.OrderByDescending(a => a.IsPinned).ThenByDescending(a => a.PublishedDate).ToListAsync();

            ViewBag.Paged = all.Skip((page - 1) * pageSize).Take(pageSize).ToList();
            ViewBag.Total = all.Count;
            ViewBag.Pages = (int)Math.Ceiling((double)all.Count / pageSize);
            ViewBag.Page = page;
            ViewBag.Search = search;
            ViewBag.Priority = priority;
            ViewBag.Urgent = all.Count(a => a.Priority == "Urgent");
            ViewBag.Pinned = all.Count(a => a.IsPinned);

            return View("~/Areas/Student/Views/Announcements/Index.cshtml");
        }
    }
}