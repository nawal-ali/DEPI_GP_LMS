using LMSProject.AI.Services;
using LMSProject.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MLSCore.IdentityModel;

namespace LMSProject.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class AutomationController : BaseController
    {
        private readonly WeeklyReportService _reports;
        private readonly EmailService _email;
        private readonly IConfiguration _cfg;

        public AutomationController(WeeklyReportService reports,
            EmailService email, UserManager<ApplicationUser> um,
            IConfiguration cfg) : base(um)
        { _reports = reports; _email = email; _cfg = cfg; }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        [AllowAnonymous]
        public async Task<IActionResult> SendWeeklyReports()
        {
            var expectedKey = _cfg["N8N:WebhookSecret"] ?? "change-this-secret";

            // n8n actually sends the key as "x-n8n-api-key"
            var receivedKey =
                Request.Headers["x-n8n-api-key"].FirstOrDefault() ??
                Request.Headers["X-N8N-API-Key"].FirstOrDefault() ??
                Request.Headers["X-N8N-Key"].FirstOrDefault() ??
                string.Empty;

            bool validKey = receivedKey == expectedKey;
            bool adminUser = User.Identity?.IsAuthenticated == true &&
                             (User.IsInRole("Admin") || User.IsInRole("SuperAdmin"));

            if (!validKey && !adminUser)
                return Unauthorized(new { error = "Unauthorized." });

            try
            {
                await _reports.SendAllReportsAsync();
                if (validKey)
                    return Ok(new { success = true, message = "Reports sent.", sentAt = DateTime.UtcNow });
                TempData["Success"] = "Weekly reports sent to all parents.";
                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, error = ex.Message });
            }
        }

        [HttpPost, ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,SuperAdmin")]
        public async Task<IActionResult> TestEmail(string toEmail, string toName)
        {
            try
            {
                await _email.SendAsync(toEmail, toName, "TOTC LMS — Email Test",
                    "<h2>Email working!</h2>");
                TempData["Success"] = $"Test email sent to {toEmail}.";
            }
            catch (Exception ex) { TempData["Error"] = $"Failed: {ex.Message}"; }
            return RedirectToAction("Index", "Home");
        }
    }
}