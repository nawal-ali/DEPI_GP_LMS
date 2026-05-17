using LMSProject.Areas.Parent.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MLSCore.IdentityModel;
using MLSEF;

// ═══════════════════════════════════════════════════════════════════════════════
// AnnouncementsController
// ═══════════════════════════════════════════════════════════════════════════════
namespace LMSProject.Areas.Parent.Controllers
{
    [Area("Parent")]
    [Authorize(Roles = "Parent")]
    public class AnnouncementsController : Controller
    {
        private readonly AppDbContext _db;

        public AnnouncementsController(AppDbContext db) => _db = db;

        public async Task<IActionResult> Index(string search = "", string priority = "", int page = 1)
        {
            ViewData["Title"] = "Announcements";
            int pageSize = 6;

            var query = _db.Announcements
                .Where(a => a.CurrentState == 1 && a.IsActive
                         && (a.ExpiryDate == null || a.ExpiryDate > DateTime.Now)
                         && (a.TargetAudience == "All" || a.TargetAudience == "Parents"))
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(a => a.Title.Contains(search) || a.Content.Contains(search));
            if (!string.IsNullOrWhiteSpace(priority))
                query = query.Where(a => a.Priority == priority);

            var all = await query
                .OrderByDescending(a => a.IsPinned)
                .ThenByDescending(a => a.PublishedDate)
                .ToListAsync();

            var vm = new ParentAnnouncementVM
            {
                Paged = all.Skip((page - 1) * pageSize).Take(pageSize).ToList(),
                TotalCount = all.Count,
                TotalPages = (int)Math.Ceiling((double)all.Count / pageSize),
                CurrentPage = page,
                SearchTerm = search,
                SelectedPriority = priority,
                UrgentCount = all.Count(a => a.Priority == "Urgent"),
                PinnedCount = all.Count(a => a.IsPinned)
            };
            return View(vm);
        }
    }
}

// ═══════════════════════════════════════════════════════════════════════════════
// ProfileController
// ═══════════════════════════════════════════════════════════════════════════════
namespace LMSProject.Areas.Parent.Controllers
{
    [Area("Parent")]
    [Authorize(Roles = "Parent")]
    public class ProfileController : Controller
    {
        private readonly AppDbContext _db;
        private readonly UserManager<ApplicationUser> _um;

        public ProfileController(AppDbContext db, UserManager<ApplicationUser> um)
        { _db = db; _um = um; }

        public async Task<IActionResult> Index()
        {
            ViewData["Title"] = "My Profile";
            var user = await _um.GetUserAsync(User);
            var parent = await _db.Parents
                .Include(p => p.Children)
                .FirstOrDefaultAsync(p => p.UserId == user!.Id);
            if (parent == null) return RedirectToAction("Index", "Home");

            var vm = new ParentProfileVM
            {
                Id = parent.Id,
                FullName = parent.FullName,
                Email = parent.Email,
                Phone = parent.PhoneNumber,
                AltPhone = parent.AlternativePhoneNumber,
                Occupation = parent.Occupation,
                Address = parent.Address,
                City = parent.City,
                NationalId = parent.NationalId,
                Relationship = parent.Relationship,
                ImageName = parent.ImageName,
                ChildrenCount = parent.Children.Count(c => c.CurrentState == 1)
            };
            return View(vm);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ParentProfileVM vm)
        {
            var user = await _um.GetUserAsync(User);
            var parent = await _db.Parents.FirstOrDefaultAsync(p => p.UserId == user!.Id);
            if (parent == null) return NotFound();

            parent.FullName = vm.FullName;
            parent.PhoneNumber = vm.Phone;
            parent.AlternativePhoneNumber = vm.AltPhone;
            parent.Occupation = vm.Occupation;
            parent.Address = vm.Address;
            parent.City = vm.City;
            parent.UpdatedDate = DateTime.Now;

            if (vm.Image != null)
            {
                var uploads = Path.Combine("wwwroot", "Images", "images");
                Directory.CreateDirectory(uploads);
                var fileName = $"{Guid.NewGuid()}{Path.GetExtension(vm.Image.FileName)}";
                using var stream = System.IO.File.Create(Path.Combine(uploads, fileName));
                await vm.Image.CopyToAsync(stream);
                parent.ImageName = $"Images/images/{fileName}";
            }

            await _db.SaveChangesAsync();
            TempData["Success"] = "Profile updated successfully.";
            return RedirectToAction("Index");
        }
    }
}

// ═══════════════════════════════════════════════════════════════════════════════
// ReportController — generates weekly PDF report
// ═══════════════════════════════════════════════════════════════════════════════
namespace LMSProject.Areas.Parent.Controllers
{
    [Area("Parent")]
    [Authorize(Roles = "Parent")]
    public class ReportController : Controller
    {
        private readonly AppDbContext _db;
        private readonly UserManager<ApplicationUser> _um;

        public ReportController(AppDbContext db, UserManager<ApplicationUser> um)
        { _db = db; _um = um; }

        private async Task<MLSCore.Models.TbParent?> GetParent()
        {
            var user = await _um.GetUserAsync(User);
            if (user == null) return null;
            return await _db.Parents
                .Include(p => p.Children).ThenInclude(c => c.Grade)
                .FirstOrDefaultAsync(p => p.UserId == user.Id);
        }

        // Preview page
        public async Task<IActionResult> Index()
        {
            ViewData["Title"] = "Weekly Progress Report";
            var vm = await BuildReport();
            return View(vm);
        }

        // Download as PDF (rendered HTML → PDF via browser print)
        public async Task<IActionResult> Download()
        {
            var vm = await BuildReport();
            return View("ReportPdf", vm);
        }

        private async Task<WeeklyReportVM> BuildReport()
        {
            var parent = await GetParent();
            if (parent == null) return new WeeklyReportVM();

            var weekStart = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek);
            var weekEnd = weekStart.AddDays(6);

            var children = parent.Children.Where(c => c.CurrentState == 1).ToList();
            var childIds = children.Select(c => c.Id).ToList();

            var enrollments = await _db.StudentCourses
                .Where(sc => childIds.Contains(sc.StId))
                .Include(sc => sc.Course)
                .ToListAsync();

            var allCourseIds = enrollments.Select(e => e.CourseId).Distinct().ToList();

            var examResults = await _db.StudentTests
                .Where(st => childIds.Contains(st.StudentId))
                .Include(st => st.Test)
                .OrderByDescending(st => st.JoinDate)
                .ToListAsync();

            var assignments = await _db.Assignments
                .Where(a => allCourseIds.Contains(a.CourseId) && a.CurrentState == 1)
                .Include(a => a.Course)
                .Include(a => a.Submissions.Where(s => childIds.Contains(s.StudentId) && s.CurrentState == 1))
                .ToListAsync();

            var childReports = children.Select(child =>
            {
                var childCourseIds = enrollments.Where(e => e.StId == child.Id).Select(e => e.CourseId).ToList();
                var childExams = examResults.Where(e => e.StudentId == child.Id).ToList();
                var childAssigns = assignments.Where(a => childCourseIds.Contains(a.CourseId)).ToList();
                var childSubs = childAssigns.SelectMany(a => a.Submissions.Where(s => s.StudentId == child.Id)).ToList();
                var weekSubs = childSubs.Where(s => s.SubmittedAt >= weekStart && s.SubmittedAt <= weekEnd).Count();
                double avg = childExams.Any()
                    ? childExams.Average(e => e.Test?.TotalMarks > 0 ? e.Score / e.Test.TotalMarks * 100 : 0) : 0;
                int missing = childAssigns.Count(a =>
                    !childSubs.Any(s => s.AssignmentId == a.Id) && DateTime.Now > a.Deadline);

                return new ChildReportVM
                {
                    FullName = child.FullName,
                    Grade = child.Grade?.Name ?? "",
                    ExamResults = childExams.Select(e => new ExamResultVM
                    {
                        ExamTitle = e.Test?.Title ?? "",
                        Score = e.Score,
                        TotalMarks = e.Test?.TotalMarks ?? 0,
                        TakenAt = e.JoinDate
                    }).ToList(),
                    Assignments = childAssigns.Select(a =>
                    {
                        var sub = childSubs.FirstOrDefault(s => s.AssignmentId == a.Id);
                        return new AssignmentStatusVM
                        {
                            Title = a.Title,
                            CourseName = a.Course?.Name ?? "",
                            Deadline = a.Deadline,
                            TotalMarks = a.TotalMarks,
                            IsSubmitted = sub != null,
                            IsLate = sub?.IsLate ?? false,
                            Score = sub?.Marks,
                            IsGraded = sub?.Marks.HasValue ?? false
                        };
                    }).ToList(),
                    OverallAvg = Math.Round(avg, 1),
                    SubmittedThisWeek = weekSubs,
                    MissingWork = missing,
                    PerformanceSummary = avg switch
                    {
                        >= 85 => "Excellent performance this period. Keep up the great work!",
                        >= 70 => "Good academic progress. Minor areas to strengthen.",
                        >= 50 => "Average performance. Needs more focus and practice.",
                        _ => "Requires immediate attention and additional support."
                    }
                };
            }).ToList();

            return new WeeklyReportVM
            {
                ParentName = parent.FullName,
                ReportDate = DateTime.Now,
                WeekStart = weekStart,
                WeekEnd = weekEnd,
                Children = childReports
            };
        }
    }
}