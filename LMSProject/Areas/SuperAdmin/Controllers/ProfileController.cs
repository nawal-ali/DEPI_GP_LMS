using LMSProject.Areas.SuperAdmin.Services;
using LMSProject.Areas.SuperAdmin.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MLSCore.IdentityModel;

namespace LMSProject.Areas.SuperAdmin.Controllers
{
    [Area("SuperAdmin")]
    [Authorize(Roles = "SuperAdmin")]
    public class ProfileController : Controller
    {
        private readonly SuperAdminDataService _data;
        private readonly UserManager<ApplicationUser> _um;   // ← injected

        public ProfileController(SuperAdminDataService data,
                                 UserManager<ApplicationUser> um)
        { _data = data; _um = um; }

        public IActionResult Index()
        {
            ViewData["Title"] = "My Profile";
            ViewData["Breadcrumb"] = new List<(string, string?)> { ("Profile", null) };
            var vm = _data.GetSuperAdminProfile();
            return View(vm);
        }

        public IActionResult Edit()
        {
            ViewData["Title"] = "Profile Settings";
            ViewData["Breadcrumb"] = new List<(string, string?)>
            {
                ("Profile", Url.Action("Index", "Profile", new { area = "SuperAdmin" })),
                ("Settings", null)
            };
            var profile = _data.GetSuperAdminProfile();
            var vm = new EditProfileVM
            {
                FullName = profile.FullName,
                Email = profile.Email,
                Phone = profile.Phone,
                Address = profile.Address,
                Bio = profile.Bio
            };
            return View(vm);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult Edit(EditProfileVM vm)
        {
            if (!ModelState.IsValid) return View(vm);
            _data.UpdateSuperAdminProfile(vm);
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
                return RedirectToAction("Edit");
            }

            if (NewPassword.Length < 8)
            {
                TempData["Error"] = "New password must be at least 8 characters.";
                return RedirectToAction("Edit");
            }

            if (NewPassword == CurrentPassword)
            {
                TempData["Error"] = "New password must be different from your current password.";
                return RedirectToAction("Edit");
            }

            if (NewPassword != ConfirmPassword)
            {
                TempData["Error"] = "New password and confirmation do not match.";
                return RedirectToAction("Edit");
            }

            // ← fixed: use _um (injected in constructor above)
            var user = await _um.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login", "Account", new { area = "" });

            var result = await _um.ChangePasswordAsync(user, CurrentPassword, NewPassword);

            TempData[result.Succeeded ? "Success" : "Error"] = result.Succeeded
                ? "Password updated successfully."
                : string.Join(" ", result.Errors.Select(e => e.Description));

            return RedirectToAction("Edit");
        }
    }
}