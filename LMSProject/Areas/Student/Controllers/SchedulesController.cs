using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MLSCore.IdentityModel;
using MLSEF;
using LMSProject.Controllers;

namespace LMSProject.Areas.Student.Controllers
{
    [Area("Student")]
    [Authorize(Roles = "Student")]
    public class SchedulesController : BaseController
    {
        private readonly AppDbContext _db;
        public SchedulesController(AppDbContext db, UserManager<ApplicationUser> um) : base(um)
        { _db = db; }

        public async Task<IActionResult> Index()
        {
            var student = await _db.Students
                .FirstOrDefaultAsync(s => s.UserId == CurrentUserId && s.CurrentState == 1);
            if (student == null) return View("~/Areas/Student/Views/Schedules/Index.cshtml", new List<MLSCore.Models.TbScheduleSession>());

            var enrolledCourseIds = await _db.StudentCourses
                .Where(sc => sc.StId == student.Id)
                .Select(sc => sc.CourseId)
                .ToListAsync();

            var sessions = await _db.ScheduleSessions
                .Include(s => s.Course).ThenInclude(c => c.Grade).ThenInclude(g => g.Stage)
                .Include(s => s.Course).ThenInclude(c => c.Term)
                .Include(s => s.Course).ThenInclude(c => c.SubSubject).ThenInclude(ss => ss.Subject)
                .Include(s => s.Course).ThenInclude(c => c.Instructor)
                .Where(s => s.CurrentState == 1 && s.Course.CurrentState == 1
                         && enrolledCourseIds.Contains(s.CourseId))
                .ToListAsync();

            return View("~/Areas/Student/Views/Schedules/Index.cshtml", sessions);
        }
    }
}