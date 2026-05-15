using LMSProject.Areas.Instructor.ViewModel;
using LMSProject.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MLSCore.IdentityModel;
using MLSEF;

namespace LMSProject.Areas.Instructor.Controllers
{
    [Area("Instructor")]
    [Authorize(Roles = "Instructor")]
    public class HomeController : BaseController
    {
        private readonly AppDbContext _context;

        public HomeController(AppDbContext context, UserManager<ApplicationUser> userManager)
            : base(userManager)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var userId = CurrentUserId;
            var instructor = await _context.Instructors.FirstOrDefaultAsync(i => i.UserId == userId);
            if (instructor == null) return RedirectToAction("Login", "Account", new { area = "" });

            var instructorId = instructor.Id;

            var courseIds = await _context.Courses
                .Where(c => c.InstructorId == instructorId && c.CurrentState == 1)
                .Select(c => c.Id)
                .ToListAsync();

            var totalStudents = await _context.StudentCourses
                .Where(sc => courseIds.Contains(sc.CourseId))
                .Select(sc => sc.StId)
                .Distinct()
                .CountAsync();

            var totalExams = await _context.Tests
                .Where(t => courseIds.Contains(t.CourseId) && t.CurrentState == 1)
                .CountAsync();

            var totalAssignments = await _context.Assignments
                .Where(a => courseIds.Contains(a.CourseId) && a.CurrentState == 1)
                .CountAsync();

            var totalMaterials = await _context.CourseMaterials
                .Where(m => courseIds.Contains(m.CourseId) && m.CurrentState == 1)
                .CountAsync();

            var pendingSubmissions = await _context.AssignmentSubmissions
                .Where(s => courseIds.Contains(s.Assignment.CourseId) && s.Marks == null && s.CurrentState == 1)
                .CountAsync();

            // Recent activity
            var recentActivity = new List<RecentActivityVM>();

            var recentSubmissions = await _context.AssignmentSubmissions
                .Where(s => courseIds.Contains(s.Assignment.CourseId) && s.CurrentState == 1)
                .Include(s => s.Student)
                .Include(s => s.Assignment)
                .OrderByDescending(s => s.SubmittedAt)
                .Take(3)
                .ToListAsync();

            foreach (var sub in recentSubmissions)
                recentActivity.Add(new RecentActivityVM
                {
                    Message = $"{sub.Student?.FullName} submitted \"{sub.Assignment?.Title}\"",
                    Time = sub.SubmittedAt.HasValue ? sub.SubmittedAt.Value.ToString("MMM d, h:mm tt") : "",
                    Icon = "fa-file-upload",
                    Color = "#5B72EE"
                });

            var recentAttempts = await _context.StudentTests
                .Where(st => courseIds.Contains(st.Test.CourseId))
                .Include(st => st.Student)
                .Include(st => st.Test)
                .OrderByDescending(st => st.JoinDate)
                .Take(3)
                .ToListAsync();

            foreach (var att in recentAttempts)
                recentActivity.Add(new RecentActivityVM
                {
                    Message = $"{att.Student?.FullName} took exam \"{att.Test?.Title}\" — {att.Score:0}pts",
                    Time = att.JoinDate.ToString("MMM d, h:mm tt"),
                    Icon = "fa-clipboard-check",
                    Color = "#33EFA0"
                });

            recentActivity = recentActivity
                .OrderByDescending(a => a.Time)
                .Take(6)
                .ToList();

            // Upcoming deadlines
            var upcomingExams = await _context.Tests
                .Where(t => courseIds.Contains(t.CourseId) && t.CurrentState == 1
                         && t.Deadline.HasValue && t.Deadline > DateTime.Now)
                .Include(t => t.Questions)
                .OrderBy(t => t.Deadline)
                .Take(3)
                .Select(t => new UpcomingDeadlineVM
                {
                    Title = t.Title,
                    CourseName = _context.Courses.Where(c => c.Id == t.CourseId).Select(c => c.Name).FirstOrDefault() ?? "",
                    Deadline = t.Deadline.Value,
                    Type = "Exam",
                    BadgeColor = "#5B72EE"
                })
                .ToListAsync();

            var upcomingAssignments = await _context.Assignments
                .Where(a => courseIds.Contains(a.CourseId) && a.CurrentState == 1
                         && a.Deadline > DateTime.Now)
                .OrderBy(a => a.Deadline)
                .Take(3)
                .Select(a => new UpcomingDeadlineVM
                {
                    Title = a.Title,
                    CourseName = _context.Courses.Where(c => c.Id == a.CourseId).Select(c => c.Name).FirstOrDefault() ?? "",
                    Deadline = a.Deadline,
                    Type = "Assignment",
                    BadgeColor = "#F48C06"
                })
                .ToListAsync();

            var deadlines = upcomingExams.Concat(upcomingAssignments)
                .OrderBy(d => d.Deadline)
                .Take(5)
                .ToList();

            var vm = new InstructorDashboardVM
            {
                TotalCourses = courseIds.Count,
                TotalStudents = totalStudents,
                TotalExams = totalExams,
                TotalAssignments = totalAssignments,
                TotalMaterials = totalMaterials,
                PendingSubmissions = pendingSubmissions,
                RecentActivity = recentActivity,
                UpcomingDeadlines = deadlines
            };

            ViewData["Title"] = "Instructor Dashboard";
            return View(vm);
        }
    }
}
