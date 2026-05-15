using LMSProject.Areas.SuperAdmin.Services;
using LMSProject.Areas.SuperAdmin.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MLSCore.IdentityModel;
using MLSCore.Models;
using MLSEF;

namespace LMSProject.Areas.SuperAdmin.Controllers
{
    [Area("SuperAdmin")]
    [Authorize(Roles = "SuperAdmin")]
    public class UsersController : Controller
    {
        private readonly SuperAdminDataService _data;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly AppDbContext _context;

        public UsersController(SuperAdminDataService data, UserManager<ApplicationUser> userManager, AppDbContext context)
        {
            _data = data;
            _userManager = userManager;
            _context = context;
        }

        // ── Students tab ──────────────────────────────────────────────────
        public IActionResult Students(string search = "", string grade = "",
                                      string status = "", string section = "", int page = 1)
        {
            ViewData["Title"] = "Users — Students";
            ViewData["Breadcrumb"] = new List<(string, string?)> { ("Users", null), ("Students", null) };
            var vm = _data.GetStudents(search, grade, status, section, page);
            return View(vm);
        }

        // ── Teachers tab ──────────────────────────────────────────────────
        public async Task<IActionResult> Teachers(string search = "", string subject = "",
                                                  string status = "", string grade = "", int page = 1)
        {
            ViewData["Title"] = "Users — Teachers";
            ViewData["Breadcrumb"] = new List<(string, string?)> { ("Users", null), ("Teachers", null) };
            var vm = await _data.GetTeachersAsync(search, subject, status, grade, page);
            return View(vm);
        }

        // ── Parents tab ───────────────────────────────────────────────────
        public IActionResult Parents(string search = "", string status = "",
                                     string occ = "", string children = "", int page = 1)
        {
            ViewData["Title"] = "Users — Parents";
            ViewData["Breadcrumb"] = new List<(string, string?)> { ("Users", null), ("Parents", null) };
            var vm = _data.GetParents(search, status, occ, children, page);
            return View(vm);
        }

        // ── Admins tab ────────────────────────────────────────────────────
        public IActionResult Admins(string search = "", string status = "",
                                    string access = "", string dept = "", int page = 1)
        {
            ViewData["Title"] = "Users — Admins";
            ViewData["Breadcrumb"] = new List<(string, string?)> { ("Users", null), ("Admins", null) };
            var vm = _data.GetAdmins(search, status, access, dept, page);
            return View(vm);
        }

        // ── User public profile view ──────────────────────────────────────
        public IActionResult ViewProfile(string id, string role)
        {
            ViewData["Title"] = "User Profile";
            ViewData["Breadcrumb"] = new List<(string, string?)>
            {
                ("Users", null),
                (role + "s", Url.Action(role + "s", "Users", new { area = "SuperAdmin" })),
                ("Profile", null)
            };
            var vm = _data.GetUserProfile(id, role);
            if (vm == null || string.IsNullOrEmpty(vm.Id))
            {
                TempData["Error"] = "User not found.";
                return RedirectToAction("Students");
            }
            return View(vm);
        }

        // ── Create user (POST) ────────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateUser(CreateUserVM vm)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Please fill in all required fields correctly.";
                return RedirectToAction(vm.Role + "s");
            }

            // Check email uniqueness
            var existing = await _userManager.FindByEmailAsync(vm.Email);
            if (existing != null)
            {
                TempData["Error"] = "An account with this email already exists.";
                return RedirectToAction(vm.Role + "s");
            }

            var user = new ApplicationUser
            {
                UserName = vm.Email,
                Email = vm.Email,
                PhoneNumber = vm.Phone
            };

            // "Teacher" in the UI maps to the "Instructor" identity role
            var identityRole = vm.Role == "Teacher" ? "Instructor" : vm.Role;

            var result = await _userManager.CreateAsync(user, vm.Password);
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, identityRole);

                if (identityRole == "Instructor")
                {
                    var tbInstructor = new TbInstructor
                    {
                        FullName = vm.FullName,
                        UserId = user.Id,
                        Bio = "",
                        Specialization = vm.Subject ?? "",
                        ExperienceYears = 0,
                        CurrentState = 1,
                        ImageName = ""
                    };
                    _context.Instructors.Add(tbInstructor);
                    await _context.SaveChangesAsync();
                }

                TempData["Success"] = $"{vm.Role} account for {vm.FullName} created successfully.";
            }
            else
            {
                TempData["Error"] = string.Join("; ", result.Errors.Select(e => e.Description));
            }

            return RedirectToAction(vm.Role + "s");
        }

        // ── Delete user (POST) ────────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteUser(string id, string role)
        {
            _data.DeleteUser(id, role);
            TempData["Success"] = "User removed successfully.";
            return RedirectToAction(role + "s");
        }
    }
}