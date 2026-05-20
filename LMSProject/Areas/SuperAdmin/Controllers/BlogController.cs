using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MLSCore.Models;
using MLSEF;

namespace LMSProject.Areas.SuperAdmin.Controllers
{
    [Area("SuperAdmin")]
    [Authorize(Roles = "SuperAdmin")]
    public class BlogController : Controller
    {
        private readonly AppDbContext _db;
        private readonly IWebHostEnvironment _env;

        public BlogController(AppDbContext db, IWebHostEnvironment env)
        { _db = db; _env = env; }

        // ── List ──────────────────────────────────────────────────────────
        public async Task<IActionResult> Index(string? q)
        {
            ViewData["Title"] = "Blog Management";
            var query = _db.Articles.Where(a => a.CurrentState == 1);
            if (!string.IsNullOrWhiteSpace(q))
                query = query.Where(a => a.Title.Contains(q) || (a.Excerpt != null && a.Excerpt.Contains(q)));

            var articles = await query.OrderByDescending(a => a.CreatedAt).ToListAsync();
            ViewBag.Q = q;
            return View("~/Areas/SuperAdmin/Views/Blog/Index.cshtml", articles);
        }

        // ── Create GET ────────────────────────────────────────────────────
        [HttpGet]
        public IActionResult Create()
        {
            ViewData["Title"] = "Add Article";
            return View("~/Areas/SuperAdmin/Views/Blog/Create.cshtml", new TbArticle());
        }

        // ── Create POST ───────────────────────────────────────────────────
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TbArticle vm, IFormFile? CoverFile)
        {
            ModelState.Remove(nameof(TbArticle.CoverImage));
            if (!ModelState.IsValid)
            {
                ViewData["Title"] = "Add Article";
                return View("~/Areas/SuperAdmin/Views/Blog/Create.cshtml", vm);
            }

            if (CoverFile != null && CoverFile.Length > 0)
                vm.CoverImage = await SaveImage(CoverFile);

            vm.CreatedAt = DateTime.Now;
            vm.CurrentState = 1;
            if (string.IsNullOrWhiteSpace(vm.Excerpt) && !string.IsNullOrWhiteSpace(vm.Content))
                vm.Excerpt = vm.Content.Length > 200
                    ? vm.Content[..200].TrimEnd() + "…"
                    : vm.Content;

            _db.Articles.Add(vm);
            await _db.SaveChangesAsync();
            TempData["Success"] = "Article published.";
            return RedirectToAction("Index");
        }

        // ── Edit GET ──────────────────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            ViewData["Title"] = "Edit Article";
            var a = await _db.Articles.FindAsync(id);
            if (a == null) return NotFound();
            return View("~/Areas/SuperAdmin/Views/Blog/Edit.cshtml", a);
        }

        // ── Edit POST ─────────────────────────────────────────────────────
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(TbArticle vm, IFormFile? CoverFile)
        {
            ModelState.Remove(nameof(TbArticle.CoverImage));
            if (!ModelState.IsValid)
            {
                ViewData["Title"] = "Edit Article";
                return View("~/Areas/SuperAdmin/Views/Blog/Edit.cshtml", vm);
            }

            var a = await _db.Articles.FindAsync(vm.Id);
            if (a == null) return NotFound();

            a.Title = vm.Title;
            a.Content = vm.Content;
            a.Excerpt = vm.Excerpt;
            a.UpdatedAt = DateTime.Now;

            if (string.IsNullOrWhiteSpace(a.Excerpt) && !string.IsNullOrWhiteSpace(a.Content))
                a.Excerpt = a.Content.Length > 200
                    ? a.Content[..200].TrimEnd() + "…"
                    : a.Content;

            if (CoverFile != null && CoverFile.Length > 0)
            {
                DeleteOldImage(a.CoverImage);
                a.CoverImage = await SaveImage(CoverFile);
            }

            await _db.SaveChangesAsync();
            TempData["Success"] = "Article updated.";
            return RedirectToAction("Index");
        }

        // ── Delete POST ───────────────────────────────────────────────────
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var a = await _db.Articles.FindAsync(id);
            if (a != null) { a.CurrentState = 0; await _db.SaveChangesAsync(); }
            TempData["Success"] = "Article deleted.";
            return RedirectToAction("Index");
        }

        // ── Seed (run once) ───────────────────────────────────────────────
        [HttpPost]
        public async Task<IActionResult> Seed()
        {
            if (await _db.Articles.AnyAsync())
            {
                TempData["Error"] = "Articles already seeded.";
                return RedirectToAction("Index");
            }

            var seeds = new[]
            {
                new TbArticle
                {
                    Title      = "Class adds $30 million to its balance sheet for a Zoom-friendly edtech solution",
                    Excerpt    = "Class, launched less than a year ago by Blackboard co-founder Michael Chasen, integrates exclusively with Zoom to bring LMS features into the video conferencing platform.",
                    Content    = "Class, launched less than a year ago by Blackboard co-founder Michael Chasen, integrates exclusively with Zoom to bring LMS features into the video conferencing platform. The company has now added $30 million in fresh funding to fuel its next growth phase.\n\nThe investment underscores the growing appetite for ed-tech solutions that meet educators and students where they already are — inside video calls. Class enables assignment submission, attendance tracking, polls, and live assessments directly within Zoom.\n\n\"We're not replacing your LMS,\" said Chasen, \"we're making it work where teaching actually happens.\" The company plans to use the new capital to expand its engineering team and accelerate feature development.",
                    CreatedAt  = new DateTime(2024, 3, 15),
                    CurrentState = 1
                },
                new TbArticle
                {
                    Title      = "Class Technologies Inc. Closes $30 Million Series A Financing to Meet High Demand",
                    Excerpt    = "Class Technologies Inc., the company that created Class, an advanced virtual classroom built on top of Zoom, today announced the close of its $30 Million Series A round of financing.",
                    Content    = "Class Technologies Inc., the company that created Class — an advanced virtual classroom built on top of Zoom — today announced the close of its $30 Million Series A round of financing.\n\nThe round was led by prominent Silicon Valley venture firms, with participation from several strategic ed-tech investors. This brings the company's total raised to $46 million since its founding.\n\nThe capital will be deployed across three key areas: product development, international expansion, and enterprise sales. \"The demand has been extraordinary,\" said CEO Michael Chasen. \"Schools and universities want a richer teaching experience without asking teachers to learn an entirely new platform.\"",
                    CreatedAt  = new DateTime(2024, 5, 10),
                    CurrentState = 1
                },
                new TbArticle
                {
                    Title      = "Former Blackboard CEO Raises $16M to Bring LMS Features to Zoom Classrooms",
                    Excerpt    = "This year, investors have reaped big financial returns from betting on Zoom. Now some of Zoom's earliest backers are betting on Class.",
                    Content    = "This year, investors have reaped big financial returns from betting on Zoom. Now some of Zoom's earliest backers are making a new wager: that a Zoom-native LMS can capture the attention of the world's educators.\n\nFormer Blackboard CEO Michael Chasen has raised $16 million to build exactly that. His company, Class, layers assignment management, grading, breakout rooms with structure, and attendance directly into Zoom's interface.\n\nChasen believes that the future of remote education isn't about building new platforms from scratch but about enhancing the tools that teachers and students are already comfortable with. \"Zoom became the classroom,\" he said. \"We're making it a great one.\"",
                    CreatedAt  = new DateTime(2024, 8, 22),
                    CurrentState = 1
                }
            };

            _db.Articles.AddRange(seeds);
            await _db.SaveChangesAsync();
            TempData["Success"] = "3 seed articles added.";
            return RedirectToAction("Index");
        }

        // ── Helpers ───────────────────────────────────────────────────────
        private async Task<string> SaveImage(IFormFile file)
        {
            var folder = Path.Combine(_env.WebRootPath, "Uploads", "Articles");
            Directory.CreateDirectory(folder);
            var fn = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            var path = Path.Combine(folder, fn);
            await using var stream = new FileStream(path, FileMode.Create);
            await file.CopyToAsync(stream);
            return $"Uploads/Articles/{fn}";
        }

        private void DeleteOldImage(string? img)
        {
            if (string.IsNullOrEmpty(img)) return;
            var full = Path.Combine(_env.WebRootPath, img.Replace('/', Path.DirectorySeparatorChar));
            if (System.IO.File.Exists(full)) System.IO.File.Delete(full);
        }
    }
}