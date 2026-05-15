using LMSProject.Areas.SuperAdmin.Services;
using LMSProject.Areas.SuperAdmin.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LMSProject.Areas.SuperAdmin.Controllers
{
    [Area("SuperAdmin")]
    [Authorize(Roles = "SuperAdmin")]
    public class ProfileController : Controller
    {
        private readonly SuperAdminDataService _data;
        public ProfileController(SuperAdminDataService data) => _data = data;

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
            ViewData["Breadcrumb"] = new List<(string, string?)> { ("Profile", Url.Action("Index", "Profile", new { area = "SuperAdmin" })), ("Settings", null) };
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(EditProfileVM vm)
        {
            if (!ModelState.IsValid) return View(vm);
            _data.UpdateSuperAdminProfile(vm);
            TempData["Success"] = "Profile updated successfully.";
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ChangePassword(ChangePasswordVM vm)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Please check your password entries.";
                return RedirectToAction("Edit");
            }
            if (vm.NewPassword != vm.ConfirmPassword)
            {
                TempData["Error"] = "New passwords do not match.";
                return RedirectToAction("Edit");
            }
            TempData["Success"] = "Password changed successfully.";
            return RedirectToAction("Edit");
        }
    }
}