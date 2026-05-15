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
    public class HomeController : BaseController
    {
        private readonly AppDbContext _db;

        public HomeController(AppDbContext db, UserManager<ApplicationUser> um) : base(um) => _db = db;

        public async Task<IActionResult> Index()
        {
            ViewData["Title"] = "Admin Dashboard";

            var courseIds = await _db.Courses
                .Where(c => c.CurrentState == 1).Select(c => c.Id).ToListAsync();

            var vm = new AdminDashboardVM
            {
                TotalStudents = await _db.Students.CountAsync(s => s.CurrentState == 1),
                TotalInstructors = await _db.Instructors.CountAsync(i => i.CurrentState == 1),
                TotalParents = await _db.Parents.CountAsync(),
                TotalCourses = courseIds.Count,
                TotalExams = await _db.Tests.CountAsync(t => t.CurrentState == 1),
                TotalAssignments = await _db.Assignments.CountAsync(a => a.CurrentState == 1),
                TotalSubmissions = await _db.AssignmentSubmissions.CountAsync(s => s.CurrentState == 1),
                PendingGrading = await _db.AssignmentSubmissions.CountAsync(s => s.CurrentState == 1 && s.Marks == null),
                TotalAnnouncements = await _db.Announcements.CountAsync(a => a.CurrentState == 1),
                ActiveAnnouncements = await _db.Announcements.CountAsync(a => a.CurrentState == 1 && a.IsActive),

                RecentCourses = await _db.Courses
                    .Where(c => c.CurrentState == 1)
                    .Include(c => c.Instructor)
                    .Include(c => c.StudentCourses)
                    .OrderByDescending(c => c.CreatedDate)
                    .Take(4)
                    .Select(c => new RecentCourseVM
                    {
                        Id = c.Id,
                        Name = c.Name,
                        ImageName = c.ImageName,
                        InstructorName = c.Instructor != null ? c.Instructor.FullName : "—",
                        StudentCount = c.StudentCourses != null ? c.StudentCourses.Count : 0,
                        CurrentState = c.CurrentState
                    }).ToListAsync(),

                RecentActivity = new List<RecentActivityVM>
                {
                    new() { Icon = "fa-user-plus",    Color = "#5B72EE", Message = "New student enrolled",          Time = "Just now" },
                    new() { Icon = "fa-book-open",    Color = "#29B9E7", Message = "New course created",            Time = "1 hr ago" },
                    new() { Icon = "fa-bullhorn",     Color = "#00CBB8", Message = "Announcement published",        Time = "2 hrs ago" },
                    new() { Icon = "fa-clipboard-check",Color="#33EFA0", Message = "Assignment graded",             Time = "3 hrs ago" },
                    new() { Icon = "fa-user-tie",     Color = "#F48C06", Message = "Instructor profile updated",   Time = "5 hrs ago" },
                }
            };

            return View(vm);
        }
    }
}