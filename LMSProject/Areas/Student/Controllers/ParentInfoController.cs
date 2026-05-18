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
    public class ParentInfoController : BaseController
    {
        private readonly AppDbContext _db;
        public ParentInfoController(AppDbContext db, UserManager<ApplicationUser> um) : base(um) => _db = db;

        public async Task<IActionResult> Index()
        {
            ViewData["Title"] = "My Parent";
            var user = await _userManager.GetUserAsync(User);
            var student = await _db.Students.Include(s => s.Parent).FirstOrDefaultAsync(s => s.UserId == user!.Id && s.CurrentState == 1);

            var vm = new StudentParentVM
            {
                ParentName = student?.Parent?.FullName,
                ParentEmail = student?.Parent?.Email,
                ParentPhone = student?.Parent?.PhoneNumber,
                Relationship = student?.Parent?.Relationship,
                Occupation = student?.Parent?.Occupation,
                ImageName = student?.Parent?.ImageName
            };

            return View("~/Areas/Student/Views/Parent/Index.cshtml", vm);
        }
    }
}
