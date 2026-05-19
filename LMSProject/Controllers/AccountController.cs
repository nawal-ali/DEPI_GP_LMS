using AutoMapper;
using LMSProject.Areas.Admin.Helpers;
using LMSProject.Areas.Admin.ViewModel;
using LMSProject.ViewModel;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using LMSProject.AI.Services;
using MLSCore;
using MLSCore.IdentityModel;
using MLSCore.Models;

namespace LMSProject.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _usermanager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly EmailService _email;

        public AccountController(IMapper mapper, IUnitOfWork unitOfWork,
                                 UserManager<ApplicationUser> usermanager,
                                 SignInManager<ApplicationUser> signInManager,
                                 EmailService email)
        {
            _usermanager = usermanager;
            _signInManager = signInManager;
            _mapper = mapper;
            _unitOfWork = unitOfWork;
            _email = email;
        }

        // ── Register — disabled. Only SuperAdmin can create accounts. ─────────
        public IActionResult Register()
            => RedirectToAction("Login");

        [HttpPost]
        public IActionResult Register(object model)
            => RedirectToAction("Login");

        // ── Login ───────────────────────────────────────────────────────────
        public IActionResult Login() => View();

        [HttpPost]
        public async Task<IActionResult> Login(LoginVM model)
        {
            if (!ModelState.IsValid)
                return View(model);

            // 1. Resolve user — try email first, then username
            ApplicationUser? user = await _usermanager.FindByEmailAsync(model.EmailOrUsername);
            if (user == null)
                user = await _usermanager.FindByNameAsync(model.EmailOrUsername);

            if (user == null)
            {
                ModelState.AddModelError("", "Invalid email/username or password.");
                return View(model);
            }

            // 2. Sign in
            var result = await _signInManager.PasswordSignInAsync(
                user.UserName!,
                model.Password,
                model.RememberMe,
                lockoutOnFailure: false);

            if (!result.Succeeded)
            {
                ModelState.AddModelError("", "Invalid email/username or password.");
                return View(model);
            }

            // 3. Redirect to the correct dashboard based on role
            if (await _usermanager.IsInRoleAsync(user, "SuperAdmin"))
                return RedirectToAction("Index", "Home", new { area = "SuperAdmin" });

            if (await _usermanager.IsInRoleAsync(user, "Admin"))
                return RedirectToAction("Index", "Home", new { area = "Admin" });

            if (await _usermanager.IsInRoleAsync(user, "Instructor"))
                return RedirectToAction("Index", "Home", new { area = "Instructor" });

            if (await _usermanager.IsInRoleAsync(user, "Parent"))
                return RedirectToAction("Index", "Home", new { area = "Parent" });

            if (await _usermanager.IsInRoleAsync(user, "Student"))
                return RedirectToAction("Index", "Home", new { area = "Student" });

            return RedirectToAction("Index", "Home", new { area = "" });

        }

        // ── Logout ──────────────────────────────────────────────────────────
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login");
        }

        // ── Forgot Password ──────────────────────────────────────────────────
        [HttpGet]
        public IActionResult ForgotPassword() => View();

        [HttpPost]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordVM model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _usermanager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                // Don't reveal that the user doesn't exist
                TempData["ForgotSuccess"] = "If this email exists, a temporary password has been sent.";
                return RedirectToAction("Login");
            }

            // Generate a random temporary password
            var tempPassword = "Temp@" + Guid.NewGuid().ToString("N")[..8].ToUpper() + "1!";

            // Reset to the temporary password
            var token = await _usermanager.GeneratePasswordResetTokenAsync(user);
            var result = await _usermanager.ResetPasswordAsync(user, token, tempPassword);

            if (!result.Succeeded)
            {
                ModelState.AddModelError("", "Could not reset password. Please try again.");
                return View(model);
            }

            // Send the temp password by email
            try
            {
                var name = user.FullName ?? user.Email ?? "User";
                var html = "<h2 style='color:#2F327D;'>🔐 Temporary Password</h2>" +
                           "<p>Hi <strong>" + System.Net.WebUtility.HtmlEncode(name) + "</strong>,</p>" +
                           "<p>Your temporary password for TOTC LMS is:</p>" +
                           "<div style='background:#f4f6fb;border:2px solid #5B72EE;border-radius:12px;" +
                           "padding:1rem 1.5rem;font-size:1.3rem;font-weight:700;color:#2F327D;" +
                           "letter-spacing:2px;text-align:center;margin:1rem 0;'>" +
                           System.Net.WebUtility.HtmlEncode(tempPassword) + "</div>" +
                           "<p>Please log in with this password and <strong>change it immediately</strong> from your profile for security.</p>" +
                           "<p style='color:#9ca3af;font-size:.85rem;'>If you did not request this, contact your administrator.</p>";

                await _email.SendAsync(user.Email!, name,
                    "TOTC LMS — Your Temporary Password", html);
            }
            catch
            {
                // Email failed — still show success but warn
                TempData["ForgotSuccess"] = "Password reset. Email could not be sent — contact your administrator.";
                return RedirectToAction("Login");
            }

            TempData["ForgotSuccess"] = "A temporary password has been sent to your email. Please log in and change it.";
            return RedirectToAction("Login");
        }

        // ── Reset Password ───────────────────────────────────────────────────
        [HttpGet]
        public IActionResult ResetPassword(string token, string email)
            => View(new ResetPasswordVM { Token = token, Email = email });

        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordVM model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _usermanager.FindByEmailAsync(model.Email);
            if (user == null)
                return RedirectToAction("Login");

            var result = await _usermanager.ResetPasswordAsync(user, model.Token, model.Password);
            if (result.Succeeded)
                return RedirectToAction("Login");

            foreach (var error in result.Errors)
                ModelState.AddModelError("", error.Description);

            return View(model);
        }

        public IActionResult Index() => View();
    }
}