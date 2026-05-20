using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MLSCore.IdentityModel;
using MLSCore.Models;
using MLSEF;

namespace LMSProject.Controllers
{
    public class HomeController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly AppDbContext _db;

        public HomeController(UserManager<ApplicationUser> userManager, AppDbContext db)
        {
            _userManager = userManager;
            _db = db;
        }

        public async Task<IActionResult> Index()
        {
            // Latest 4 articles for news section
            ViewBag.LatestArticles = await _db.Articles
                .Where(a => a.CurrentState == 1)
                .OrderByDescending(a => a.CreatedAt)
                .Take(4)
                .ToListAsync();
            return View();
        }

        public IActionResult About() => View();

        // ── Blog: list all articles ───────────────────────────────────────
        public async Task<IActionResult> Blog()
        {
            var articles = await _db.Articles
                .Where(a => a.CurrentState == 1)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();
            return View(articles);
        }

        // ── Blog: single article ──────────────────────────────────────────
        [Route("Blog/{id:int}/{slug?}")]
        public async Task<IActionResult> Article(int id)
        {
            var article = await _db.Articles
                .FirstOrDefaultAsync(a => a.Id == id && a.CurrentState == 1);
            if (article == null) return NotFound();

            ViewBag.Related = await _db.Articles
                .Where(a => a.Id != id && a.CurrentState == 1)
                .OrderByDescending(a => a.CreatedAt)
                .Take(3)
                .ToListAsync();

            return View(article);
        }

        // ── Contact form submission ───────────────────────────────────────
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Contact(TbContactForm model)
        {
            if (!ModelState.IsValid)
            {
                TempData["ContactError"] = "Please fill all required fields correctly.";
                return RedirectToAction("Index", new { section = "contact" });
            }
            model.SubmittedAt = DateTime.Now;
            model.IsRead = false;
            _db.ContactForms.Add(model);
            await _db.SaveChangesAsync();
            TempData["ContactSuccess"] = "Thank you! Your message has been received. We'll be in touch soon.";
            return RedirectToAction("Index", new { section = "contact" });
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error() => View("~/Views/Shared/Error.cshtml");
    }
}