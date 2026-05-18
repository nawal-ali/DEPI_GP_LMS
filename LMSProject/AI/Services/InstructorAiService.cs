using LMSProject.AI.Models;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;

namespace LMSProject.AI.Services
{
    public class InstructorAiService
    {
        private readonly GithubAiService _ai;
        private readonly DocumentProcessingService _docs;
        private readonly MongoDbService _mongo;

        public InstructorAiService(GithubAiService ai,
            DocumentProcessingService docs, MongoDbService mongo)
        { _ai = ai; _docs = docs; _mongo = mongo; }

        // ── Generate exam from uploaded material ───────────────────────────
        public async Task<string> GenerateExamAsync(string instructorUserId,
            string fileId, int mcqCount = 5, int tfCount = 3)
        {
            // Instructors upload to their own namespace
            var chunks = await _mongo.Chunks
                .Find(c => c.FileId == fileId && c.StudentUserId == instructorUserId)
                .Limit(10)
                .ToListAsync();

            if (!chunks.Any())
                return "No content found. Please upload a document first.";

            var text = string.Join("\n\n", chunks.Select(c => c.Content));

            var prompt = $"""
                Based on the following educational content, generate an exam with:
                - {mcqCount} Multiple Choice Questions (MCQ) — each with 4 options (A/B/C/D) and mark the correct answer
                - {tfCount} True/False questions — each with the correct answer marked

                Format:
                === MCQ ===
                1. [Question]
                   A) ...  B) ...  C) ...  D) ...
                   ✓ Correct: [letter]

                === True / False ===
                1. [Statement] → [True / False]

                Content:
                {text}
                """;

            return await _ai.ChatAsync(
                new List<(string, string)> { ("user", prompt) },
                "You are an expert educational assessment creator. Generate clear, accurate exam questions.");
        }

        // ── Process instructor file upload (reuses student pipeline) ──────
        public async Task<(bool ok, string msg, UploadedFile? file)> ProcessUploadAsync(
            IFormFile upload, string instructorUserId)
        {
            // Reuse the same pipeline — instructor userId = studentUserId field
            return await _docs.ProcessUploadAsync(upload, instructorUserId);
        }

        // ── Get instructor's uploaded files ────────────────────────────────
        public async Task<List<UploadedFile>> GetFilesAsync(string instructorUserId)
        {
            return await _mongo.Files
                .Find(f => f.StudentUserId == instructorUserId && f.IsProcessed)
                .SortByDescending(f => f.UploadedAt)
                .ToListAsync();
        }
    }
}