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
    public class ProfileController : BaseController
    {
        private readonly AppDbContext _db;

        public ProfileController(UserManager<ApplicationUser> um, AppDbContext db) : base(um)
            => _db = db;

        public async Task<IActionResult> Index()
        {
            ViewData["Title"] = "My Profile";
            var user = await _userManager.GetUserAsync(User);
            var instructor = await _db.Instructors
                .FirstOrDefaultAsync(i => i.UserId == user!.Id && i.CurrentState == 1);

            ViewBag.User = user;
            ViewBag.Instructor = instructor;
            return View("~/Areas/Instructor/Views/Profile/Index.cshtml");
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string FullName, string Phone,
            string? Specialization, string? Bio, IFormFile? Image)
        {
            var user = await _userManager.GetUserAsync(User);
            var instructor = await _db.Instructors
                .FirstOrDefaultAsync(i => i.UserId == user!.Id && i.CurrentState == 1);

            if (instructor != null)
            {
                instructor.FullName = FullName;
                instructor.Specialization = Specialization;
                instructor.Bio = Bio;
                instructor.UpdatedDate = DateTime.Now;

                if (Image != null)
                {
                    var folder = Path.Combine("wwwroot", "Images", "images");
                    Directory.CreateDirectory(folder);
                    var fn = $"{Guid.NewGuid()}{Path.GetExtension(Image.FileName)}";
                    using var stream = System.IO.File.Create(Path.Combine(folder, fn));
                    await Image.CopyToAsync(stream);
                    instructor.ImageName = $"Images/images/{fn}";
                }
            }

            if (user != null)
            {
                user.PhoneNumber = Phone;
                await _userManager.UpdateAsync(user);
            }

            await _db.SaveChangesAsync();
            TempData["Success"] = "Profile updated successfully.";
            return RedirectToAction("Index");
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(
            string CurrentPassword, string NewPassword, string ConfirmPassword)
        {
            if (string.IsNullOrEmpty(CurrentPassword) ||
                string.IsNullOrEmpty(NewPassword) ||
                string.IsNullOrEmpty(ConfirmPassword))
            {
                TempData["Error"] = "All three password fields are required.";
                return RedirectToAction("Index");
            }

            if (NewPassword.Length < 8)
            {
                TempData["Error"] = "New password must be at least 8 characters.";
                return RedirectToAction("Index");
            }

            if (NewPassword == CurrentPassword)
            {
                TempData["Error"] = "New password must be different from your current password.";
                return RedirectToAction("Index");
            }

            if (NewPassword != ConfirmPassword)
            {
                TempData["Error"] = "New password and confirmation do not match.";
                return RedirectToAction("Index");
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login", "Account", new { area = "" });

            var result = await _userManager.ChangePasswordAsync(user, CurrentPassword, NewPassword);

            TempData[result.Succeeded ? "Success" : "Error"] = result.Succeeded
                ? "Password updated successfully."
                : string.Join(" ", result.Errors.Select(e => e.Description));

            return RedirectToAction("Index");
        }
    }
}