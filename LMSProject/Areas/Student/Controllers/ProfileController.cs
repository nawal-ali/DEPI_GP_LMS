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
    public class ProfileController : BaseController
    {
        private readonly AppDbContext _db;
        public ProfileController(AppDbContext db, UserManager<ApplicationUser> um) : base(um) => _db = db;

        public async Task<IActionResult> Index()
        {
            ViewData["Title"] = "My Profile";
            var user = await _userManager.GetUserAsync(User);
            var student = await _db.Students.Include(s => s.Grade).FirstOrDefaultAsync(s => s.UserId == user!.Id && s.CurrentState == 1);
            if (student is null) return NotFound();

            return View("~/Areas/Student/Views/Profile/Index.cshtml", new StudentProfileVM
            {
                StudentId = student.Id,
                FullName = student.FullName,
                Email = user?.Email ?? "",
                Phone = user?.PhoneNumber,
                GradeName = student.Grade?.Name,
                ImageName = student.ImageName
            });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(StudentProfileVM vm)
        {
            var user = await _userManager.GetUserAsync(User);
            var student = await _db.Students.FirstOrDefaultAsync(s => s.Id == vm.StudentId && s.UserId == user!.Id);
            if (student is null) return NotFound();

            student.FullName = vm.FullName;
            student.UpdatedDate = DateTime.Now;

            if (vm.Image != null)
            {
                var folder = Path.Combine("wwwroot", "Images", "images");
                Directory.CreateDirectory(folder);
                var fn = $"{Guid.NewGuid()}{Path.GetExtension(vm.Image.FileName)}";
                using var st = System.IO.File.Create(Path.Combine(folder, fn));
                await vm.Image.CopyToAsync(st);
                student.ImageName = $"Images/images/{fn}";
            }

            if (!string.IsNullOrEmpty(vm.NewPassword))
            {
                var result = await _userManager.ChangePasswordAsync(user!, vm.CurrentPassword ?? "", vm.NewPassword);
                if (!result.Succeeded) { TempData["Error"] = string.Join(", ", result.Errors.Select(e => e.Description)); return RedirectToAction("Index"); }
            }

            if (user != null) { user.PhoneNumber = vm.Phone; await _userManager.UpdateAsync(user); }
            await _db.SaveChangesAsync();
            TempData["Success"] = "Profile updated successfully.";
            return RedirectToAction("Index");
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(string CurrentPassword, string NewPassword, string ConfirmPassword)
        {
            if (string.IsNullOrEmpty(CurrentPassword) || string.IsNullOrEmpty(NewPassword) || string.IsNullOrEmpty(ConfirmPassword))
            {
                TempData["Error"] = "All three password fields are required.";
                return RedirectToAction("Index");
            }

            if (NewPassword != ConfirmPassword)
            {
                TempData["Error"] = "New password and confirmation do not match.";
                return RedirectToAction("Index");
            }

            if (NewPassword == CurrentPassword)
            {
                TempData["Error"] = "New password must be different from your current password.";
                return RedirectToAction("Index");
            }

            if (NewPassword.Length < 8)
            {
                TempData["Error"] = "New password must be at least 8 characters.";
                return RedirectToAction("Index");
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account", new { area = "" });

            var result = await _userManager.ChangePasswordAsync(user, CurrentPassword, NewPassword);
            if (result.Succeeded)
            {
                TempData["Success"] = "Password updated successfully.";
            }
            else
            {
                TempData["Error"] = string.Join(" ", result.Errors.Select(e => e.Description));
            }

            return RedirectToAction("Index");
        }
    }
}