using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MLSCore.IdentityModel;
using MLSEF;
using LMSProject.Controllers;

namespace LMSProject.Areas.Instructor.Controllers
{
    [Area("Instructor")]
    [Authorize(Roles = "Instructor")]
    public class SchedulesController : BaseController
    {
        private readonly AppDbContext _db;
        public SchedulesController(AppDbContext db, UserManager<ApplicationUser> um) : base(um)
        { _db = db; }

        public async Task<IActionResult> Index()
        {
            var instructor = await _db.Instructors
                .FirstOrDefaultAsync(i => i.UserId == CurrentUserId && i.CurrentState == 1);
            if (instructor == null) return View("~/Areas/Instructor/Views/Schedules/Index.cshtml", new List<MLSCore.Models.TbScheduleSession>());

            var sessions = await _db.ScheduleSessions
                .Include(s => s.Course).ThenInclude(c => c.Grade).ThenInclude(g => g.Stage)
                .Include(s => s.Course).ThenInclude(c => c.Term)
                .Include(s => s.Course).ThenInclude(c => c.SubSubject).ThenInclude(ss => ss.Subject)
                .Include(s => s.Course).ThenInclude(c => c.Instructor)
                .Where(s => s.CurrentState == 1 && s.Course.CurrentState == 1
                         && s.Course.InstructorId == instructor.Id)
                .ToListAsync();

            return View("~/Areas/Instructor/Views/Schedules/Index.cshtml", sessions);
        }
    }
}