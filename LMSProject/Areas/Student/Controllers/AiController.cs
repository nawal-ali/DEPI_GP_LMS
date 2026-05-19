using LMSProject.AI.Services;
using LMSProject.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MLSCore.IdentityModel;
using MLSEF;

namespace LMSProject.Areas.Student.Controllers
{
    [Area("Student")]
    [Authorize(Roles = "Student")]
    public class AiController : BaseController
    {
        private readonly AiChatService _chat;
        private readonly DocumentProcessingService _docs;
        private readonly AppDbContext _db;

        public AiController(AiChatService chat, DocumentProcessingService docs,
            AppDbContext db, UserManager<ApplicationUser> um) : base(um)
        { _chat = chat; _docs = docs; _db = db; }

        // ── Chat page (returns partial for the floating panel) ─────────────
        [HttpGet]
        public async Task<IActionResult> Session()
        {
            var session = await _chat.GetOrCreateSessionAsync(CurrentUserId);
            var history = await _chat.GetHistoryAsync(session.Id, CurrentUserId);
            var files = await _docs.GetStudentFilesAsync(CurrentUserId);

            ViewBag.SessionId = session.Id;
            ViewBag.History = history;
            ViewBag.Files = files;
            return PartialView("~/Areas/Student/Views/Ai/_ChatPanel.cshtml");
        }

        // ── Send message ───────────────────────────────────────────────────
        [HttpPost, IgnoreAntiforgeryToken]
        public async Task<IActionResult> Chat([FromBody] ChatRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.Message))
                return BadRequest(new { error = "Message cannot be empty." });

            var session = await _chat.GetOrCreateSessionAsync(CurrentUserId);
            var answer = await _chat.ChatAsync(
                req.Message, session.Id, CurrentUserId, req.FileId);

            return Ok(new { answer, sessionId = session.Id });
        }

        // ── Upload file ────────────────────────────────────────────────────
        [HttpPost, IgnoreAntiforgeryToken]
        public async Task<IActionResult> Upload(IFormFile file)
        {
            if (file == null) return BadRequest(new { error = "No file provided." });

            var (ok, msg, uploaded) = await _docs.ProcessUploadAsync(file, CurrentUserId);
            if (!ok) return BadRequest(new { error = msg });

            return Ok(new { message = msg, fileId = uploaded!.Id, fileName = uploaded.OriginalName });
        }

        // ── Delete uploaded file ───────────────────────────────────────────
        [HttpPost, IgnoreAntiforgeryToken]
        public async Task<IActionResult> DeleteFile([FromBody] FileIdRequest req)
        {
            await _docs.DeleteFileAsync(req.FileId, CurrentUserId);
            return Ok(new { message = "File deleted." });
        }

        // ── AI actions (summarize / mcq / key points) ──────────────────────
        [HttpPost, IgnoreAntiforgeryToken]
        public async Task<IActionResult> Action([FromBody] AiActionRequest req)
        {
            var result = req.Action switch
            {
                "summarize" => await _chat.SummarizeAsync(req.FileId, CurrentUserId),
                "mcq" => await _chat.GenerateMcqAsync(req.FileId, CurrentUserId),
                "keypoints" => await _chat.KeyPointsAsync(req.FileId, CurrentUserId),
                "studyplan" => await _chat.GenerateStudyPlanAsync(CurrentUserId),
                _ => "Unknown action."
            };
            return Ok(new { result });
        }

        // ── Get student's uploaded files ───────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> Files()
        {
            var files = await _docs.GetStudentFilesAsync(CurrentUserId);
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
    }

    // ── Request DTOs ───────────────────────────────────────────────────────
    public record ChatRequest(string Message, string? FileId, string? SessionId);
    public record FileIdRequest(string FileId);
    public record AiActionRequest(string Action, string FileId);
}