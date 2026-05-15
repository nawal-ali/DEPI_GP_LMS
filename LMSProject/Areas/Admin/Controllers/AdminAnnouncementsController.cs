using LMSProject.Areas.Admin.ViewModels;
using LMSProject.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MLSCore.IdentityModel;
using MLSCore.Models;
using MLSEF;

namespace LMSProject.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public class AdminAnnouncementsController : BaseController
    {
        private readonly AppDbContext _db;

        public AdminAnnouncementsController(AppDbContext db, UserManager<ApplicationUser> um) : base(um) => _db = db;

        public async Task<IActionResult> Index(string search = "", string priority = "",
                                               string audience = "", int page = 1)
        {
            ViewData["Title"] = "Announcements";

            var all = await _db.Announcements
                .Where(a => a.CurrentState == 1)
                .OrderByDescending(a => a.IsPinned)
                .ThenByDescending(a => a.PublishedDate)
                .ToListAsync();

            var filtered = all.AsEnumerable();
            if (!string.IsNullOrWhiteSpace(search))
                filtered = filtered.Where(a =>
                    a.Title.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    a.Content.Contains(search, StringComparison.OrdinalIgnoreCase));
            if (!string.IsNullOrWhiteSpace(priority))
                filtered = filtered.Where(a => a.Priority == priority);
            if (!string.IsNullOrWhiteSpace(audience))
                filtered = filtered.Where(a => a.TargetAudience == audience);

            var filteredList = filtered.ToList();

            var vm = new AnnouncementListVM
            {
                Items = all,
                Filtered = filteredList,
                SearchTerm = search,
                SelectedPriority = priority,
                SelectedAudience = audience,
                CurrentPage = page,
                TotalActive = all.Count(a => a.IsActive),
                TotalPinned = all.Count(a => a.IsPinned),
                TotalUrgent = all.Count(a => a.Priority == "Urgent")
            };

            return View("~/Areas/Admin/Views/Announcements/Index.cshtml", vm);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateAnnouncementVM vm)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Title and Content are required.";
                return RedirectToAction("Index");
            }

            _db.Announcements.Add(new TbAnnouncement
            {
                Title = vm.Title,
                Content = vm.Content,
                Description = vm.Description ?? "",
                TargetAudience = vm.TargetAudience,
                Priority = vm.Priority,
                Category = vm.Category,
                IsPinned = vm.IsPinned,
                ExpiryDate = vm.ExpiryDate,
                PublishedDate = DateTime.Now,
                IsActive = true,
                CurrentState = 1,
                CreatedBy = User.Identity?.Name ?? "Admin",
                CreatedByUserId = CurrentUserId,
                CreatedDate = DateTime.Now,
                ImageUrl = "",
                AttachmentUrl = ""
            });

            await _db.SaveChangesAsync();
            TempData["Success"] = "Announcement published successfully.";
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            ViewData["Title"] = "Edit Announcement";
            var a = await _db.Announcements.FindAsync(id);
            if (a == null) return NotFound();

            return View("~/Areas/Admin/Views/Announcements/Edit.cshtml", new EditAnnouncementVM
            {
                Id = a.Id,
                Title = a.Title,
                Content = a.Content,
                Description = a.Description,
                TargetAudience = a.TargetAudience,
                Priority = a.Priority,
                Category = a.Category,
                IsPinned = a.IsPinned,
                ExpiryDate = a.ExpiryDate
            });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditAnnouncementVM vm)
        {
            if (!ModelState.IsValid)
            {
                ViewData["Title"] = "Edit Announcement";
                return View("~/Areas/Admin/Views/Announcements/Edit.cshtml", vm);
            }

            var a = await _db.Announcements.FindAsync(vm.Id);
            if (a == null) return NotFound();

            a.Title = vm.Title;
            a.Content = vm.Content;
            a.Description = vm.Description ?? "";
            a.TargetAudience = vm.TargetAudience;
            a.Priority = vm.Priority;
            a.Category = vm.Category;
            a.IsPinned = vm.IsPinned;
            a.ExpiryDate = vm.ExpiryDate;
            a.UpdatedBy = User.Identity?.Name ?? "Admin";
            a.UpdatedDate = DateTime.Now;

            await _db.SaveChangesAsync();
            TempData["Success"] = "Announcement updated.";
            return RedirectToAction("Index");
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var a = await _db.Announcements.FindAsync(id);
            if (a != null) { a.CurrentState = 0; await _db.SaveChangesAsync(); }
            TempData["Success"] = "Announcement deleted.";
            return RedirectToAction("Index");
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> TogglePin(int id)
        {
            var a = await _db.Announcements.FindAsync(id);
            if (a != null) { a.IsPinned = !a.IsPinned; await _db.SaveChangesAsync(); }
            TempData["Success"] = a?.IsPinned == true ? "Pinned." : "Unpinned.";
            return RedirectToAction("Index");
        }
    }
}