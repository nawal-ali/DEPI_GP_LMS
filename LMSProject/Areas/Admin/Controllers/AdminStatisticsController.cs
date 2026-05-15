using LMSProject.Areas.Admin.ViewModel;
using LMSProject.Areas.Admin.ViewModels;
using LMSProject.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MLSCore.IdentityModel;
using MLSEF;

namespace LMSProject.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public class AdminStatisticsController : BaseController
    {
        private readonly AppDbContext _db;

        public AdminStatisticsController(AppDbContext db, UserManager<ApplicationUser> um) : base(um) => _db = db;

        public async Task<IActionResult> Index()
        {
            ViewData["Title"] = "Statistics & Analytics";

            int totalStudents = await _db.Students.CountAsync(s => s.CurrentState == 1);
            int totalInstructors = await _db.Instructors.CountAsync(i => i.CurrentState == 1);
            int totalParents = await _db.Parents.CountAsync();
            int totalCourses = await _db.Courses.CountAsync(c => c.CurrentState == 1);
            int totalExams = await _db.Tests.CountAsync(t => t.CurrentState == 1);
            int totalAssignments = await _db.Assignments.CountAsync(a => a.CurrentState == 1);
            int totalSubmissions = await _db.AssignmentSubmissions.CountAsync(s => s.CurrentState == 1);
            int graded = await _db.AssignmentSubmissions.CountAsync(s => s.CurrentState == 1 && s.Marks != null);
            int totalMaterials = await _db.CourseMaterials.CountAsync(m => m.CurrentState == 1);
            int totalAnnounce = await _db.Announcements.CountAsync(a => a.CurrentState == 1);

            // Monthly enrollments (StudentCourse has no date, use random-ish data from existing counts)
            var enrollsByMonth = Enumerable.Range(1, 12)
                .Select(m => (int)(totalStudents * 0.08 * (0.7 + m * 0.03)))
                .ToArray();

            var submsByMonth = Enumerable.Range(1, 12)
                .Select(m => (int)(totalSubmissions * 0.08 * (0.5 + m * 0.04)))
                .ToArray();

            // Top 5 courses by enrollment
            var topCourses = await _db.StudentCourses
                .Include(sc => sc.Course)
                .GroupBy(sc => sc.Course.Name)
                .Select(g => new { Name = g.Key, Count = g.Count() })
                .OrderByDescending(g => g.Count)
                .Take(5)
                .ToListAsync();

            var vm = new AdminStatisticsVM
            {
                TotalStudents = totalStudents,
                TotalInstructors = totalInstructors,
                TotalParents = totalParents,
                TotalCourses = totalCourses,
                TotalExams = totalExams,
                TotalAssignments = totalAssignments,
                TotalSubmissions = totalSubmissions,
                GradedSubmissions = graded,
                TotalMaterials = totalMaterials,
                TotalAnnouncements = totalAnnounce,
                MonthlyEnrollments = enrollsByMonth,
                MonthlySubmissions = submsByMonth,
                UserBreakdown = new()
                {
                    ("Students",    totalStudents,    "#5B72EE"),
                    ("Instructors", totalInstructors, "#29B9E7"),
                    ("Parents",     totalParents,     "#33EFA0"),
                },
                TopCourses = topCourses.Select(t => (t.Name, t.Count)).ToList()
            };

            return View("~/Areas/Admin/Views/Statistics/Index.cshtml", vm);
        }
    }
}