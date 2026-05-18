using LMSProject.Areas.Student.ViewModels;
using LMSProject.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MLSCore.IdentityModel;
using MLSEF;

namespace LMSProject.Areas.Student.Controllers
{
    [Area("Student")]
    [Authorize(Roles = "Student")]
    public class HomeController : BaseController
    {
        private readonly AppDbContext _db;

        public HomeController(AppDbContext db, UserManager<ApplicationUser> um) : base(um) => _db = db;

        private async Task<MLSCore.Models.TbStudent?> GetStudent()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return null;
            return await _db.Students
                .Include(s => s.Grade)
                .Include(s => s.Parent)
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.UserId == user.Id && s.CurrentState == 1);
        }

        public async Task<IActionResult> Index()
        {
            ViewData["Title"] = "Dashboard";
            var student = await GetStudent();
            if (student is null) return RedirectToAction("Login", "Account", new { area = "" });

            var enrollments = await _db.StudentCourses
                .Where(sc => sc.StId == student.Id)
                .Include(sc => sc.Course).ThenInclude(c => c.Instructor)
                .ToListAsync();

            var courseIds = enrollments.Select(e => e.CourseId).ToList();

            var examResults = await _db.StudentTests
                .Where(st => st.StudentId == student.Id)
                .Include(st => st.Test)
                .OrderByDescending(st => st.JoinDate)
                .ToListAsync();

            var assignments = await _db.Assignments
                .Where(a => courseIds.Contains(a.CourseId) && a.CurrentState == 1)
                .Include(a => a.Submissions.Where(s => s.StudentId == student.Id && s.CurrentState == 1))
                .Include(a => a.Course)
                .ToListAsync();

            var activeExams = await _db.Tests
                .Where(t => courseIds.Contains(t.CourseId) && t.CurrentState == 1
                         && (t.Deadline == null || t.Deadline > DateTime.Now))
                .ToListAsync();
            var takenIds = examResults.Select(e => e.TestId).ToHashSet();

            var announcements = await _db.Announcements
                .Where(a => a.CurrentState == 1 && a.IsActive
                         && (a.ExpiryDate == null || a.ExpiryDate > DateTime.Now)
                         && (a.TargetAudience == "All" || a.TargetAudience == "Students"))
                .OrderByDescending(a => a.PublishedDate)
                .Take(5).ToListAsync();

            var materials = await _db.CourseMaterials
                .Where(m => courseIds.Contains(m.CourseId) && m.CurrentState == 1)
                .Include(m => m.Course)
                .OrderByDescending(m => m.CreatedDate)
                .Take(4).ToListAsync();

            // Upcoming deadlines
            var deadlines = new List<UpcomingDeadlineVM>();
            foreach (var exam in activeExams.Where(e => !takenIds.Contains(e.Id) && e.Deadline.HasValue))
                deadlines.Add(new UpcomingDeadlineVM
                {
                    Title = exam.Title,
                    Type = "Exam",
                    Deadline = exam.Deadline!.Value,
                    CourseName = enrollments.FirstOrDefault(e => e.CourseId == exam.CourseId)?.Course?.Name ?? ""
                });
            foreach (var a in assignments.Where(a => a.Deadline > DateTime.Now && !a.Submissions.Any()))
                deadlines.Add(new UpcomingDeadlineVM { Title = a.Title, Type = "Assignment", Deadline = a.Deadline, CourseName = a.Course?.Name ?? "" });

            double examAvg = examResults.Any()
                ? examResults.Average(e => e.Test?.TotalMarks > 0 ? e.Score / e.Test.TotalMarks * 100 : 0) : 0;

            var vm = new StudentDashboardVM
            {
                StudentName = student.FullName,
                ImageName = student.ImageName,
                GradeName = student.Grade?.Name,
                EnrolledCourses = courseIds.Count,
                CompletedExams = examResults.Count,
                PendingAssignments = assignments.Count(a => !a.Submissions.Any() && a.Deadline > DateTime.Now),
                AvailableExams = activeExams.Count(e => !takenIds.Contains(e.Id)),
                ExamAverage = Math.Round(examAvg, 1),
                Announcements = announcements.Count,
                UpcomingDeadlines = deadlines.OrderBy(d => d.Deadline).Take(6).ToList(),
                RecentResults = examResults.Take(5).Select(e => new RecentResultVM
                {
                    Title = e.Test?.Title ?? "",
                    Score = e.Score,
                    TotalMarks = e.Test?.TotalMarks ?? 0,
                    Date = e.JoinDate
                }).ToList(),
                RecentAnnouncements = announcements.Take(3).Select(a => new RecentAnnouncementVM
                {
                    Title = a.Title,
                    Priority = a.Priority ?? "Normal",
                    Date = a.PublishedDate
                }).ToList(),
                RecentMaterials = materials.Select(m => new RecentMaterialVM
                {
                    Title = m.Title,
                    CourseName = m.Course?.Name ?? "",
                    Type = m.MaterialType.ToString(),
                    Date = m.CreatedDate ?? DateTime.Now
                }).ToList()
            };

            return View("~/Areas/Student/Views/Home/Index.cshtml", vm);
        }
    }
}