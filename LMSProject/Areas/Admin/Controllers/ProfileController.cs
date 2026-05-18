using LMSProject.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MLSCore.IdentityModel;

namespace LMSProject.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public class ProfileController : BaseController
    {
        public ProfileController(UserManager<ApplicationUser> um) : base(um) { }

        public async Task<IActionResult> Index()
        {
            ViewData["Title"] = "My Profile";
            var user = await _userManager.GetUserAsync(User);
            ViewBag.User = user;
            return View("~/Areas/Admin/Views/Profile/Index.cshtml");
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string FullName, string Phone)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account", new { area = "" });

            user.FullName = FullName;
            user.PhoneNumber = Phone;
            await _userManager.UpdateAsync(user);

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