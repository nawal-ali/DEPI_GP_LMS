using LMSProject.AI.Services;
using LMSProject.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MLSCore.IdentityModel;

namespace LMSProject.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public class AutomationController : BaseController
    {
        private readonly WeeklyReportService _reports;
        private readonly EmailService _email;

        public AutomationController(WeeklyReportService reports,
            EmailService email, UserManager<ApplicationUser> um) : base(um)
        { _reports = reports; _email = email; }

        // ── Trigger weekly reports manually ───────────────────────────────
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> SendWeeklyReports()
        {
            await _reports.SendAllReportsAsync();
            TempData["Success"] = "Weekly reports sent to all parents.";
            return RedirectToAction("Index", "Home");
        }

        // ── Test email service ─────────────────────────────────────────────
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> TestEmail(string toEmail, string toName)
        {
            try
            {
                await _email.SendAsync(toEmail, toName,
                    "TOTC LMS — Email Test",
                    "<h2>✅ Email service is working correctly!</h2><p>Your SMTP configuration is valid.</p>");
                TempData["Success"] = $"Test email sent to {toEmail}.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Email failed: {ex.Message}";
            }
            return RedirectToAction("Index", "Home");
        }
    }
}