using LMSProject.AI.Services;
using LMSProject.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MLSCore.IdentityModel;

namespace LMSProject.Areas.Instructor.Controllers
{
    [Area("Instructor")]
    [Authorize(Roles = "Instructor")]
    public class AiController : BaseController
    {
        private readonly InstructorAiService _ai;

        public AiController(InstructorAiService ai, UserManager<ApplicationUser> um)
            : base(um) => _ai = ai;

        // ── Exam Generator page ────────────────────────────────────────────
        public async Task<IActionResult> ExamGenerator()
        {
            ViewData["Title"] = "AI Exam Generator";
            var files = await _ai.GetFilesAsync(CurrentUserId);
            ViewBag.Files = files;
            return View("~/Areas/Instructor/Views/Ai/ExamGenerator.cshtml");
        }

        // ── Upload material ────────────────────────────────────────────────
        [HttpPost]
        public async Task<IActionResult> Upload(IFormFile file)
        {
            if (file == null) return BadRequest(new { error = "No file." });
            var (ok, msg, f) = await _ai.ProcessUploadAsync(file, CurrentUserId);
            if (!ok) return BadRequest(new { error = msg });
            return Ok(new { message = msg, fileId = f!.Id, fileName = f.OriginalName });
        }

        // ── Generate exam ──────────────────────────────────────────────────
        [HttpPost]
        public async Task<IActionResult> Generate([FromBody] GenerateRequest req)
        {
            var result = await _ai.GenerateExamAsync(
                CurrentUserId, req.FileId, req.McqCount, req.TfCount);
            return Ok(new { result });
        }
    }

    public record GenerateRequest(string FileId, int McqCount = 5, int TfCount = 3);
}