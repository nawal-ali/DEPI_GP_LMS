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
        //public async Task<string> GenerateExamAsync(string instructorUserId,
        //    string fileId, int mcqCount = 5, int tfCount = 3)
        //{
        //    // Instructors upload to their own namespace
        //    var chunks = await _mongo.Chunks
        //        .Find(c => c.FileId == fileId && c.StudentUserId == instructorUserId)
        //        .Limit(10)
        //        .ToListAsync();

        //    if (!chunks.Any())
        //        return "⚠️ No text content found in this file.\n\n" +
        //               "Possible reasons:\n" +
        //               "• The PDF is scanned/image-based (no text layer)\n" +
        //               "• The file is empty or corrupted\n\n" +
        //               "Solution: Upload a PDF with selectable text (not a scanned image). " +
        //               "Try opening the PDF and selecting text with your mouse — if you cannot select text, it is image-based.";

        //    var text = string.Join("\n\n", chunks.Select(c => c.Content));

        //    var prompt = $"""
        //        Based on the following educational content, generate an exam with:
        //        - {mcqCount} Multiple Choice Questions (MCQ) — each with 4 options (A/B/C/D) and mark the correct answer
        //        - {tfCount} True/False questions — each with the correct answer marked

        //        Format:
        //        === MCQ ===
        //        1. [Question]
        //           A) ...  B) ...  C) ...  D) ...
        //           ✓ Correct: [letter]

        //        === True / False ===
        //        1. [Statement] → [True / False]

        //        Content:
        //        {text}
        //        """;

        //    return await _ai.ChatAsync(
        //        new List<(string, string)> { ("user", prompt) },
        //        "You are an expert educational assessment creator. Generate clear, accurate exam questions.");
        //}


        public async Task<string> GenerateExamAsync(
    string instructorUserId,
    string fileId,
    int mcqCount = 5,
    int tfCount = 3)
        {
            try
            {
                Console.WriteLine("=== GENERATE EXAM SERVICE STARTED ===");
                Console.WriteLine($"Instructor: {instructorUserId}");
                Console.WriteLine($"FileId: {fileId}");

                // Try getting chunks if available
                var chunks = await _mongo.Chunks
                    .Find(c => c.FileId == fileId && c.StudentUserId == instructorUserId)
                    .Limit(10)
                    .ToListAsync();

                Console.WriteLine($"Chunks found: {chunks.Count}");

                string text = "";

                // If chunks exist use them
                if (chunks.Any())
                {
                    text = string.Join("\n\n", chunks.Select(c => c.Content));
                }

                // If extraction failed DON'T STOP
                // Still continue to AI workflow
                if (string.IsNullOrWhiteSpace(text))
                {
                    Console.WriteLine("WARNING: No extracted text found.");
                    text = "No readable extracted text was found in the uploaded file. Try generating questions anyway.";
                }

                var prompt = $"""
Based on the following educational content, generate an exam with:

- {mcqCount} Multiple Choice Questions (MCQ)
- {tfCount} True/False questions

Rules:
- Each MCQ must contain 4 options (A/B/C/D)
- Mark the correct answer clearly
- Questions should be educational and relevant
- Keep formatting clean and readable

Format exactly like this:

=== MCQ ===

1. Question text
A) Option
B) Option
C) Option
D) Option
Answer: B

=== True / False ===

1. Statement here
Answer: True

Educational Content:
{text}
""";

                Console.WriteLine("=== SENDING REQUEST TO AI ===");

                // CALL AI / N8N
                var result = await _ai.ChatAsync(
                    new List<(string, string)>
                    {
                ("user", prompt)
                    },
                    "You are an expert educational assessment creator."
                );

                Console.WriteLine("=== AI RESPONSE RECEIVED ===");
                Console.WriteLine(result);

                return result ?? "No response returned from AI.";
            }
            catch (Exception ex)
            {
                Console.WriteLine("=== GENERATE EXAM ERROR ===");
                Console.WriteLine(ex.ToString());

                return $"Generation failed: {ex.Message}";
            }
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
                .Find(f => f.StudentUserId == instructorUserId)
                .SortByDescending(f => f.UploadedAt)
                .ToListAsync();
        }
    }
}