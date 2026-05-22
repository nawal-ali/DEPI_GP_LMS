using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MLSCore.IdentityModel;
using MLSEF;
using LMSProject.Controllers;

namespace LMSProject.Areas.Parent.Controllers
{
    [Area("Parent")]
    [Authorize(Roles = "Parent")]
    public class SchedulesController : BaseController
    {
        private readonly AppDbContext _db;
        public SchedulesController(AppDbContext db, UserManager<ApplicationUser> um) : base(um)
        { _db = db; }

        public async Task<IActionResult> Index()
        {
            var parent = await _db.Parents
                .FirstOrDefaultAsync(p => p.UserId == CurrentUserId && p.CurrentState == 1);
            if (parent == null) return View("~/Areas/Parent/Views/Schedules/Index.cshtml", new List<MLSCore.Models.TbScheduleSession>());

            // Get all children
            var childIds = await _db.Students
                .Where(s => s.ParentId == parent.Id && s.CurrentState == 1)
                .Select(s => s.Id).ToListAsync();

            var enrolledCourseIds = await _db.StudentCourses
                .Where(sc => childIds.Contains(sc.StId))
                .Select(sc => sc.CourseId).Distinct().ToListAsync();

            var sessions = await _db.ScheduleSessions
                .Include(s => s.Course).ThenInclude(c => c.Grade).ThenInclude(g => g.Stage)
                .Include(s => s.Course).ThenInclude(c => c.Term)
                .Include(s => s.Course).ThenInclude(c => c.SubSubject).ThenInclude(ss => ss.Subject)
                .Include(s => s.Course).ThenInclude(c => c.Instructor)
                .Where(s => s.CurrentState == 1 && s.Course.CurrentState == 1
                         && enrolledCourseIds.Contains(s.CourseId))
                .ToListAsync();

            return View("~/Areas/Parent/Views/Schedules/Index.cshtml", sessions);
        }
    }
}