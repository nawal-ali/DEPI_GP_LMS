using LMSProject.Areas.Student.ViewModels;
using LMSProject.Controllers;
using LMSProject.Services;
using LMSProject.ViewModels.Tickets;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MLSCore.IdentityModel;
using MLSCore.Models;
using MLSEF;

namespace LMSProject.Areas.Student.Controllers
{
    [Area("Student")]
    [Authorize(Roles = "Student")]
    public class InstructorsController : BaseController
    {
        private readonly AppDbContext _db;
        public InstructorsController(AppDbContext db, UserManager<ApplicationUser> um) : base(um) => _db = db;

        public async Task<IActionResult> Index()
        {
            ViewData["Title"] = "My Instructors";
            var user = await _userManager.GetUserAsync(User);
            var student = await _db.Students.FirstOrDefaultAsync(s => s.UserId == user!.Id && s.CurrentState == 1);
            if (student is null) return NotFound();

            var enrollments = await _db.StudentCourses
                .Where(sc => sc.StId == student.Id)
                .Include(sc => sc.Course).ThenInclude(c => c.Instructor)
                .ToListAsync();

            var vm = enrollments
                .Where(en => en.Course?.Instructor != null)
                .GroupBy(en => en.Course!.InstructorId)
                .Select(g =>
                {
                    var instr = g.First().Course!.Instructor!;
                    return new InstructorCardVM
                    {
                        InstructorId = instr.Id,
                        FullName = instr.FullName,
                        ImageName = instr.ImageName,
                        Specialization = instr.Specialization,
                        Bio = instr.Bio,
                        ExperienceYears = instr.ExperienceYears,
                        CourseNames = g.Select(e => e.Course!.Name).ToList()
                    };
                }).ToList();

            return View("~/Areas/Student/Views/Instructors/Index.cshtml", vm);
        }
    }
}