using LMSProject.AI.Models;
using MongoDB.Driver;
using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.Extensions.Configuration;

namespace LMSProject.AI.Services
{
    public class InstructorAiService
    {
        private readonly DocumentProcessingService _docs;
        private readonly MongoDbService _mongo;
        private readonly IHttpClientFactory _http;
        private readonly IWebHostEnvironment _env;
        private readonly string _webhookUrl;

        public InstructorAiService(DocumentProcessingService docs,
            MongoDbService mongo, IHttpClientFactory http,
            IWebHostEnvironment env, IConfiguration cfg)
        {
            _docs = docs;
            _mongo = mongo;
            _http = http;
            _env = env;
            _webhookUrl = cfg["N8N:ExamGeneratorWebhook"] ?? "";
        }

        // ── Generate exam ─────────────────────────────────────────────────
        public async Task<string> GenerateExamAsync(
            string instructorUserId, string fileId,
            int mcqCount = 5, int tfCount = 3)
        {
            // 1. Get the file record from MongoDB
            var fileRecord = await _mongo.Files
                .Find(f => f.Id == fileId && f.StudentUserId == instructorUserId)
                .FirstOrDefaultAsync();

            if (fileRecord == null)
                return "⚠️ File not found. Please re-upload the file.";

            var fullPath = Path.Combine(_env.WebRootPath, fileRecord.StoredPath);
            if (!File.Exists(fullPath))
                return "⚠️ File missing from disk. Please re-upload the file.";

            // 2. Send file as multipart to n8n — n8n Extract from File node handles extraction
            var client = _http.CreateClient();
            client.Timeout = TimeSpan.FromSeconds(120);

            using var form = new MultipartFormDataContent();

            // Add prompt as text field (n8n will see it as file0 binary or $json.question)
            var prompt =
                $"Generate an exam with {mcqCount} Multiple Choice Questions (A/B/C/D, mark correct answer) " +
                $"and {tfCount} True/False questions. " +
                $"Use ONLY the document content provided. " +
                $"Format:\n=== MCQ ===\n1. Question\n   A) ...\n   ✓ Answer: X\n\n=== True / False ===\n1. Statement → True/False";

            form.Add(new StringContent(prompt), "question");

            // Add the PDF file
            var fileBytes = await File.ReadAllBytesAsync(fullPath);
            var fileContent = new ByteArrayContent(fileBytes);
            fileContent.Headers.ContentType = new MediaTypeHeaderValue(
                string.IsNullOrEmpty(fileRecord.MimeType) ? "application/octet-stream" : fileRecord.MimeType);
            fileContent.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data")
            {
                Name = "\"file\"",
                FileName = "\"" + fileRecord.OriginalName + "\""
            };
            form.Add(fileContent);

            var resp = await client.PostAsync(_webhookUrl, form);
            var raw = await resp.Content.ReadAsStringAsync() ?? "";

            if (string.IsNullOrWhiteSpace(raw))
                return "⚠️ n8n returned empty response. Make sure the workflow is Published.";

            if (raw.TrimStart().StartsWith("<"))
                return "⚠️ n8n returned an error page. Use the production webhook URL (not webhook-test).";

            // 3. Extract output text from n8n JSON response
            try
            {
                using var doc = JsonDocument.Parse(raw);
                var root = doc.RootElement;
                var fields = new[] { "output", "text", "result", "answer", "message" };

                if (root.ValueKind == JsonValueKind.Array && root.GetArrayLength() > 0)
                {
                    var first = root[0];
                    foreach (var f in fields)
                        if (first.TryGetProperty(f, out var v) && v.ValueKind == JsonValueKind.String)
                            return v.GetString()!;
                }
                if (root.ValueKind == JsonValueKind.Object)
                {
                    foreach (var f in fields)
                        if (root.TryGetProperty(f, out var v) && v.ValueKind == JsonValueKind.String)
                            return v.GetString()!;
                }
            }
            catch { }

            return raw;
        }

        // ── Upload file ───────────────────────────────────────────────────
        public async Task<(bool ok, string msg, UploadedFile? file)> ProcessUploadAsync(
            IFormFile upload, string instructorUserId)
            => await _docs.ProcessUploadAsync(upload, instructorUserId);

        // ── List uploaded files ───────────────────────────────────────────
        public async Task<List<UploadedFile>> GetFilesAsync(string instructorUserId)
            => await _mongo.Files
                .Find(f => f.StudentUserId == instructorUserId)
                .SortByDescending(f => f.UploadedAt)
                .ToListAsync();
    }
}