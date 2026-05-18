using LMSProject.Areas.Parent.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MLSCore.IdentityModel;
using MLSEF;

namespace LMSProject.Areas.Parent.Controllers
{
    [Area("Parent")]
    [Authorize(Roles = "Parent")]
    public class ProfileController : Controller
    {
        private readonly AppDbContext _db;
        private readonly UserManager<ApplicationUser> _um;

        public ProfileController(AppDbContext db, UserManager<ApplicationUser> um)
        { _db = db; _um = um; }

        public async Task<IActionResult> Index()
        {
            ViewData["Title"] = "My Profile";
            var user = await _um.GetUserAsync(User);
            var parent = await _db.Parents
                .Include(p => p.Children)
                .FirstOrDefaultAsync(p => p.UserId == user!.Id);
            if (parent == null) return RedirectToAction("Index", "Home");

            var vm = new ParentProfileVM
            {
                Id = parent.Id,
                FullName = parent.FullName,
                Email = parent.Email,
                Phone = parent.PhoneNumber,
                AltPhone = parent.AlternativePhoneNumber,
                Occupation = parent.Occupation,
                Address = parent.Address,
                City = parent.City,
                NationalId = parent.NationalId,
                Relationship = parent.Relationship,
                ImageName = parent.ImageName,
                ChildrenCount = parent.Children.Count(c => c.CurrentState == 1)
            };
            return View(vm);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ParentProfileVM vm)
        {
            var user = await _um.GetUserAsync(User);
            var parent = await _db.Parents.FirstOrDefaultAsync(p => p.UserId == user!.Id);
            if (parent == null) return NotFound();

            parent.FullName = vm.FullName;
            parent.PhoneNumber = vm.Phone;
            parent.AlternativePhoneNumber = vm.AltPhone;
            parent.Occupation = vm.Occupation;
            parent.Address = vm.Address;
            parent.City = vm.City;
            parent.UpdatedDate = DateTime.Now;

            if (vm.Image != null)
            {
                var uploads = Path.Combine("wwwroot", "Images", "images");
                Directory.CreateDirectory(uploads);
                var fileName = $"{Guid.NewGuid()}{Path.GetExtension(vm.Image.FileName)}";
                using var stream = System.IO.File.Create(Path.Combine(uploads, fileName));
                await vm.Image.CopyToAsync(stream);
                parent.ImageName = $"Images/images/{fileName}";
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

            // ← fixed: use _um (consistent with the rest of this controller)
            var user = await _um.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login", "Account", new { area = "" });

            var result = await _um.ChangePasswordAsync(user, CurrentPassword, NewPassword);

            TempData[result.Succeeded ? "Success" : "Error"] = result.Succeeded
                ? "Password updated successfully."
                : string.Join(" ", result.Errors.Select(e => e.Description));

            return RedirectToAction("Index");
        }
    }
}