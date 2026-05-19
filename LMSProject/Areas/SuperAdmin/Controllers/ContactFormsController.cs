using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MLSEF;

namespace LMSProject.Areas.SuperAdmin.Controllers
{
    [Area("SuperAdmin")]
    [Authorize(Roles = "SuperAdmin")]
    public class ContactFormsController : Controller
    {
        private readonly AppDbContext _db;
        public ContactFormsController(AppDbContext db) => _db = db;

        // ── List all contact forms ─────────────────────────────────────────
        public async Task<IActionResult> Index(string search = "", bool? unread = null, int page = 1)
        {
            ViewData["Title"] = "Contact Forms";
            const int pageSize = 15;

            var q = _db.ContactForms.AsQueryable();

            if (!string.IsNullOrEmpty(search))
                q = q.Where(f => f.FullName.Contains(search) ||
                                 f.Email.Contains(search) ||
                                 f.Message.Contains(search));

            if (unread == true)
                q = q.Where(f => !f.IsRead);

            var total = await q.CountAsync();
            var items = await q.OrderByDescending(f => f.SubmittedAt)
                               .Skip((page - 1) * pageSize)
                               .Take(pageSize)
                               .ToListAsync();

            ViewBag.Search = search;
            ViewBag.Unread = unread;
            ViewBag.Page = page;
            ViewBag.TotalPages = (int)Math.Ceiling(total / (double)pageSize);
            ViewBag.UnreadCount = await _db.ContactForms.CountAsync(f => !f.IsRead);

            return View("~/Areas/SuperAdmin/Views/ContactForms/Index.cshtml", items);
        }

        // ── Mark as read ───────────────────────────────────────────────────
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkRead(int id)
        {
            var form = await _db.ContactForms.FindAsync(id);
            if (form != null) { form.IsRead = true; await _db.SaveChangesAsync(); }
            return RedirectToAction("Index");
        }

        // ── Mark all as read ───────────────────────────────────────────────
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAllRead()
        {
            await _db.ContactForms
                .Where(f => !f.IsRead)
                .ExecuteUpdateAsync(s => s.SetProperty(f => f.IsRead, true));
            TempData["Success"] = "All contact forms marked as read.";
            return RedirectToAction("Index");
        }

        // ── Delete ─────────────────────────────────────────────────────────
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var form = await _db.ContactForms.FindAsync(id);
            if (form != null) { _db.ContactForms.Remove(form); await _db.SaveChangesAsync(); }
            TempData["Success"] = "Form deleted.";
            return RedirectToAction("Index");
        }
    }
}