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
        private readonly IWebHostEnvironment _env;

        public AiController(InstructorAiService ai, UserManager<ApplicationUser> um,
            IWebHostEnvironment env) : base(um)
        { _ai = ai; _env = env; }

        // ── Page ───────────────────────────────────────────────────────────
        public async Task<IActionResult> ExamGenerator()
        {
            ViewData["Title"] = "AI Exam Generator";
            var files = await _ai.GetFilesAsync(CurrentUserId);
            ViewBag.Files = files;
            return View("~/Areas/Instructor/Views/Ai/ExamGenerator.cshtml");
        }

        // ── Upload — accepts multiple files at once ────────────────────────
        [HttpPost, IgnoreAntiforgeryToken]
        public async Task<IActionResult> Upload(List<IFormFile> files)
        {
            if (files == null || !files.Any())
                return BadRequest(new { error = "No files provided." });

            // Ensure upload directory exists
            var uploadRoot = Path.Combine(_env.WebRootPath, "Uploads", "AI");
            Directory.CreateDirectory(uploadRoot);

            var results = new List<object>();

            foreach (var file in files)
            {
                var (ok, msg, f) = await _ai.ProcessUploadAsync(file, CurrentUserId);
                results.Add(ok
                    ? new { ok = true, message = msg, fileId = f!.Id, fileName = f.OriginalName }
                    : new { ok = false, message = msg, fileId = "", fileName = file.FileName });
            }

            return Ok(new { results });
        }

        // ── Delete uploaded file ───────────────────────────────────────────
        [HttpPost, IgnoreAntiforgeryToken]
        public async Task<IActionResult> DeleteFile([FromBody] DeleteFileRequest req)
        {
            // Reuse the student pipeline — instructor userId maps to studentUserId field
            var docService = HttpContext.RequestServices
                .GetRequiredService<DocumentProcessingService>();
            await docService.DeleteFileAsync(req.FileId, CurrentUserId);
            return Ok(new { message = "File deleted." });
        }

        // ── Get files list ─────────────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> Files()
        {
            var files = await _ai.GetFilesAsync(CurrentUserId);
            return Ok(files.Select(f => new
            {
                f.Id,
                f.OriginalName,
                f.FileSizeBytes,
                f.ChunkCount,
                f.IsProcessed,
                f.UploadedAt
            }));
        }

        // ── Generate exam ──────────────────────────────────────────────────
        [HttpPost, IgnoreAntiforgeryToken]
        public async Task<IActionResult> Generate([FromBody] GenerateRequest req)
        {
            try
            {
                if (string.IsNullOrEmpty(req.FileId))
                    return BadRequest(new { error = "No file selected. Please select a file first." });

                var result = await _ai.GenerateExamAsync(
                    CurrentUserId, req.FileId, req.McqCount, req.TfCount);

                if (string.IsNullOrEmpty(result))
                    return Ok(new { result = "No content generated. Make sure the file has readable text." });

                return Ok(new { result });
            }
            catch (HttpRequestException ex)
            {
                // GitHub Models API error (wrong key, rate limit, etc.)
                return StatusCode(500, new { error = $"AI API error: {ex.Message}. Check your API key in appsettings.json." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"Generation failed: {ex.Message}" });
            }
        }
    }

    public record GenerateRequest(string FileId, int McqCount = 5, int TfCount = 3);
    public record DeleteFileRequest(string FileId);
}