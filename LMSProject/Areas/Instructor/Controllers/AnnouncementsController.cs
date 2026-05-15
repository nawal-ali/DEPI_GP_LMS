using LMSProject.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MLSCore.IdentityModel;
using MLSCore.Models;
using MLSEF;

namespace LMSProject.Areas.Instructor.Controllers
{
    [Area("Instructor")]
    [Authorize(Roles = "Instructor")]
    public class AnnouncementsController : BaseController
    {
        private readonly AppDbContext _context;

        public AnnouncementsController(AppDbContext context, UserManager<ApplicationUser> um)
            : base(um) => _context = context;

        public async Task<IActionResult> Index(string search = "", string priority = "",
                                               string audience = "", int page = 1)
        {
            ViewData["Title"] = "Announcements";

            // Only published, active, non-expired announcements
            var query = _context.Announcements
                .Where(a => a.CurrentState == 1
                         && a.IsActive
                         && (a.ExpiryDate == null || a.ExpiryDate > DateTime.Now)
                         && (a.TargetAudience == "All"
                             || a.TargetAudience == "Instructors"
                             || a.TargetAudience == "Teachers"))
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(a => a.Title.Contains(search) || a.Content.Contains(search));
            if (!string.IsNullOrWhiteSpace(priority))
                query = query.Where(a => a.Priority == priority);

            var all = await query
                .OrderByDescending(a => a.IsPinned)
                .ThenByDescending(a => a.PublishedDate)
                .ToListAsync();

            // Increment view count for newly fetched announcements (fire and forget)
            _ = IncrementViews(all.Select(a => a.Id).ToList());

            int pageSize = 6;
            var paged = all.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            ViewBag.All = all;
            ViewBag.Paged = paged;
            ViewBag.TotalPages = (int)Math.Ceiling((double)all.Count / pageSize);
            ViewBag.Page = page;
            ViewBag.Search = search;
            ViewBag.Priority = priority;
            ViewBag.Urgent = all.Count(a => a.Priority == "Urgent");
            ViewBag.Pinned = all.Count(a => a.IsPinned);

            return View();
        }

        private async Task IncrementViews(List<int> ids)
        {
            try
            {
                await _context.Announcements
                    .Where(a => ids.Contains(a.Id))
                    .ExecuteUpdateAsync(s => s.SetProperty(a => a.ViewCount, a => a.ViewCount + 1));
            }
            catch { /* non-critical */ }
        }
    }
}