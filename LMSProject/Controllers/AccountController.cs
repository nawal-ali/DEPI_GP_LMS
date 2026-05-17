using AutoMapper;
using LMSProject.Areas.Admin.Helpers;
using LMSProject.Areas.Admin.ViewModel;
using LMSProject.ViewModel;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
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

        public AccountController(IMapper mapper, IUnitOfWork unitOfWork,
                                 UserManager<ApplicationUser> usermanager,
                                 SignInManager<ApplicationUser> signInManager)
        {
            _usermanager = usermanager;
            _signInManager = signInManager;
            _mapper = mapper;
            _unitOfWork = unitOfWork;
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

            // Student / Parent — public home
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
                return RedirectToAction("ForgotPasswordConfirmation");

            var token = await _usermanager.GeneratePasswordResetTokenAsync(user);
            var resetLink = Url.Action("ResetPassword", "Account",
                new { token, email = user.Email }, Request.Scheme);

            ViewBag.ResetLink = resetLink;
            return View("ForgotPasswordConfirmation");
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