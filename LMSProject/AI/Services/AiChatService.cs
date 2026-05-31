using LMSProject.AI.Models;
using MongoDB.Driver;
using MLSEF;
using Microsoft.EntityFrameworkCore;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;

namespace LMSProject.AI.Services
{
    public class AiChatService
    {
        private readonly MongoDbService _mongo;
        private readonly GithubAiService _ai;
        private readonly DocumentProcessingService _docs;
        private readonly AppDbContext _db;
        private readonly IHttpClientFactory _http;
        private readonly IWebHostEnvironment _env;
        private readonly string _chatbotWebhook;
        private readonly string _toolsWebhook;

        public AiChatService(MongoDbService mongo, GithubAiService ai,
            DocumentProcessingService docs, AppDbContext db,
            IHttpClientFactory http, IWebHostEnvironment env, IConfiguration cfg)
        {
            _mongo = mongo;
            _ai = ai;
            _docs = docs;
            _db = db;
            _http = http;
            _env = env;
            _chatbotWebhook = cfg["N8N:ChatbotWebhook"] ?? "";
            _toolsWebhook   = cfg["N8N:ExamGeneratorWebhook"] ?? "";
        }

        // ── RAG Chat — sends to n8n (same pattern as InstructorAiService) ──
        public async Task<string> ChatAsync(string question, string sessionId,
            string studentUserId, string? fileId = null)
        {
            // 1. Get relevant document chunks from MongoDB
            var chunks = await _docs.RetrieveAsync(question, studentUserId, fileId, topK: 5);
            var context = chunks.Any()
                ? string.Join("\n\n---\n\n", chunks.Select(c => c.Content))
                : "";

            // 2. Get recent chat history (last 6 messages)
            var history = await _mongo.Messages
                .Find(m => m.SessionId == sessionId)
                .SortByDescending(m => m.CreatedAt)
                .Limit(6)
                .ToListAsync();
            history.Reverse();

            var historyText = history.Any()
                ? string.Join("\n", history.Select(m => $"{m.Role}: {m.Content}"))
                : "";

            // 3. Build system prompt
            var systemPrompt = chunks.Any()
                ? "You are a helpful AI study assistant. Answer questions related to math, science, English or Arabic. " +
                  "If the answer is not in the context, say so. Be concise and educational."
                : "You are a helpful AI study assistant. " +
                  "Answer questions related to math, science, English or Arabic." ;
            //? "You are a helpful AI study assistant. Answer based on the document context provided. " +
            //  "If the answer is not in the context, say so. Be concise and educational."
            //: "You are a helpful AI study assistant. " +
            //  "No documents uploaded yet. Answer general study questions. " +
            //  "Remind students they can upload PDF or DOCX files for document-specific help.";


            // 4. Send JSON to n8n — same as N8nService.TriggerAsync pattern
            string answer;
            if (!string.IsNullOrEmpty(_chatbotWebhook))
            {
                var client = _http.CreateClient();
                client.Timeout = TimeSpan.FromSeconds(120);

                var payload = new
                {
                    question = question,
                    context = context,
                    history = historyText,
                    systemPrompt = systemPrompt,
                    hasContext = chunks.Any()
                };

                var body = new StringContent(
                    JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

                var resp = await client.PostAsync(_chatbotWebhook, body);
                var raw = await resp.Content.ReadAsStringAsync() ?? "";

                answer = ExtractAnswer(raw);

                if (string.IsNullOrWhiteSpace(answer))
                    answer = "⚠️ No response from AI. Make sure the n8n chatbot workflow is Published.";
            }
            else
            {
                // Fallback to GitHub Models if webhook not configured
                var userContent = chunks.Any()
                    ? $"Context from uploaded document:\n{context}\n\nQuestion: {question}"
                    : question;
                var messages = history
                    .Select(m => (m.Role, m.Content))
                    .Append(("user", userContent))
                    .ToList();
                answer = await _ai.ChatAsync(messages, systemPrompt);
            }

            // 5. Save to MongoDB (same as before)
            await _mongo.Messages.InsertManyAsync(new[]
            {
                new ChatMessage { SessionId=sessionId, StudentUserId=studentUserId,
                                  Role="user",      Content=question,  SourceFileId=fileId },
                new ChatMessage { SessionId=sessionId, StudentUserId=studentUserId,
                                  Role="assistant", Content=answer,    SourceFileId=fileId }
            });

            await _mongo.Sessions.UpdateOneAsync(
                s => s.Id == sessionId,
                Builders<ChatSession>.Update.Set(s => s.UpdatedAt, DateTime.UtcNow));

            return answer;
        }

        // ── Extract answer from n8n JSON response ─────────────────────────
        private static string ExtractAnswer(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return "";
            if (raw.TrimStart().StartsWith("<")) return "";  // HTML error page

            try
            {
                using var doc = JsonDocument.Parse(raw);
                var root = doc.RootElement;
                var fields = new[] { "output", "text", "answer", "result", "message", "content", "response" };

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

            return raw; // plain text fallback
        }

        // ── Session management ─────────────────────────────────────────────
        public async Task<ChatSession> GetOrCreateSessionAsync(string studentUserId)
        {
            var session = await _mongo.Sessions
                .Find(s => s.StudentUserId == studentUserId)
                .SortByDescending(s => s.UpdatedAt)
                .FirstOrDefaultAsync();

            if (session == null)
            {
                session = new ChatSession { StudentUserId = studentUserId };
                await _mongo.Sessions.InsertOneAsync(session);
            }
            return session;
        }

        public async Task<List<ChatMessage>> GetHistoryAsync(string sessionId, string studentUserId)
            => await _mongo.Messages
                .Find(m => m.SessionId == sessionId && m.StudentUserId == studentUserId)
                .SortBy(m => m.CreatedAt)
                .ToListAsync();

        // ── AI Tools — send file to n8n (same approach as InstructorAiService) ──
        public Task<string> SummarizeAsync(string fileId, string studentUserId)
            => RunToolViaFile(fileId, studentUserId,
                "Summarize this document clearly and in a structured way with headings and bullet points.",
                "summarize",
                async () =>
                {
                    var chunks = await GetChunks(fileId, studentUserId, 10);
                    if (!chunks.Any()) return "No content found for this file.";
                    return await _ai.ChatAsync(
                        new List<(string, string)> { ("user", $"Summarize this document:\n{Join(chunks)}") },
                        "You are an expert summarizer. Provide a clear, structured summary.");
                });

        public Task<string> GenerateMcqAsync(string fileId, string studentUserId, int count = 5)
            => RunToolViaFile(fileId, studentUserId,
                $"Generate {count} Multiple Choice Questions from this document. " +
                $"Format each question as:\nQ: ...\nA) ...\nB) ...\nC) ...\nD) ...\nCorrect: ...",
                "mcq",
                async () =>
                {
                    var chunks = await GetChunks(fileId, studentUserId, 8);
                    if (!chunks.Any()) return "No content found.";
                    return await _ai.ChatAsync(
                        new List<(string, string)>
                        {
                            ("user", $"Generate {count} MCQs from this text. Format each:\nQ: ...\nA) ...\nB) ...\nC) ...\nD) ...\nCorrect: ...\n\nText:\n{Join(chunks)}")
                        },
                        "You are an expert teacher. Generate clear, educational MCQs.");
                });

        public Task<string> KeyPointsAsync(string fileId, string studentUserId)
            => RunToolViaFile(fileId, studentUserId,
                "Extract the 10 most important key points from this document as a numbered list.",
                "keypoints",
                async () =>
                {
                    var chunks = await GetChunks(fileId, studentUserId, 8);
                    if (!chunks.Any()) return "No content found.";
                    return await _ai.ChatAsync(
                        new List<(string, string)>
                        {
                            ("user", $"Extract the 10 most important key points as a numbered list:\n{Join(chunks)}")
                        },
                        "You are an expert educator. Extract the most important concepts.");
                });

        // ── Core: send file as multipart to n8n (identical to InstructorAiService) ──
        private async Task<string> RunToolViaFile(
            string fileId, string studentUserId,
            string prompt, string action,
            Func<Task<string>> githubFallback)
        {
            if (!string.IsNullOrEmpty(_toolsWebhook))
            {
                var fileRecord = await _mongo.Files
                    .Find(f => f.Id == fileId && f.StudentUserId == studentUserId)
                    .FirstOrDefaultAsync();

                if (fileRecord != null)
                {
                    var fullPath = Path.Combine(_env.WebRootPath, fileRecord.StoredPath);
                    if (File.Exists(fullPath))
                    {
                        var client = _http.CreateClient();
                        client.Timeout = TimeSpan.FromSeconds(120);

                        using var form = new MultipartFormDataContent();
                        form.Add(new StringContent(prompt), "question");
                        form.Add(new StringContent(action), "action");

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

                        var resp = await client.PostAsync(_toolsWebhook, form);
                        var raw  = await resp.Content.ReadAsStringAsync() ?? "";

                        if (!string.IsNullOrWhiteSpace(raw) && !raw.TrimStart().StartsWith("<"))
                        {
                            var answer = ExtractAnswer(raw);
                            if (!string.IsNullOrWhiteSpace(answer))
                                return answer;
                        }
                    }
                }
            }

            // Fallback to GitHub Models
            return await githubFallback();
        }

        public async Task<string> GenerateStudyPlanAsync(string studentUserId)
        {
            var student = await _db.Students
                .Include(s => s.Grade)
                .FirstOrDefaultAsync(s => s.UserId == studentUserId && s.CurrentState == 1);

            if (student == null) return "Student record not found.";

            var now = DateTime.Now;
            var courseIds = await _db.StudentCourses
                .Where(sc => sc.StId == student.Id)
                .Select(sc => sc.CourseId)
                .ToListAsync();

            var courseNames = await _db.Courses
                .Where(c => courseIds.Contains(c.Id))
                .ToDictionaryAsync(c => c.Id, c => c.Name);

            var exams = await _db.Tests
                .Where(t => courseIds.Contains(t.CourseId) && t.CurrentState == 1
                         && t.Deadline.HasValue && t.Deadline > now)
                .OrderBy(t => t.Deadline).Take(10).ToListAsync();

            var assignments = await _db.Assignments
                .Where(a => courseIds.Contains(a.CourseId) && a.CurrentState == 1 && a.Deadline > now)
                .Include(a => a.Submissions.Where(s => s.StudentId == student.Id))
                .OrderBy(a => a.Deadline).Take(10).ToListAsync();

            var enrolledCourseNames = courseNames.Values.ToList();
            var examLines = exams.Select(e => $"• {e.Title} [{courseNames.GetValueOrDefault(e.CourseId, "?")}] — Due {e.Deadline:MMM dd, yyyy} ({Math.Max(0, (int)(e.Deadline!.Value - now).TotalDays)} days left)");
            var pendingAssign = assignments.Where(a => !a.Submissions.Any()).ToList();
            var assignLines = pendingAssign.Select(a => $"• {a.Title} [{courseNames.GetValueOrDefault(a.CourseId, "?")}] — Due {a.Deadline:MMM dd, yyyy} ({Math.Max(0, (int)(a.Deadline - now).TotalDays)} days left)");

            var context =
                $"Student: {student.FullName}\n" +
                $"Grade: {student.Grade?.Name ?? "N/A"}\n" +
                $"Enrolled Courses: {(enrolledCourseNames.Any() ? string.Join(", ", enrolledCourseNames) : "None")}\n\n" +
                $"Upcoming Exams ({exams.Count}):\n{(exams.Any() ? string.Join("\n", examLines) : "None scheduled")}\n\n" +
                $"Pending Assignments ({pendingAssign.Count}):\n{(pendingAssign.Any() ? string.Join("\n", assignLines) : "None pending")}\n\n" +
                $"Submitted Assignments: {assignments.Count - pendingAssign.Count}";

            var systemPrompt =
                "You are a school academic advisor.\n" +
                "Create a 7-day study plan using ONLY the student's ACTUAL enrolled courses above.\n" +
                "Rules:\n- NEVER mention subjects not in the Enrolled Courses list.\n" +
                "- If no deadlines: focus on reviewing enrolled courses.\n" +
                "Format each day as:\n📅 Day N — [Weekday]\n• [Task with course name]\n\n" +
                "After Day 7, add:\n💡 Tips\n• [2-3 practical tips]\nBe concise and realistic.";

            return await _ai.ChatAsync(
                new List<(string, string)> { ("user", $"Create a study plan for:\n\n{context}") },
                systemPrompt);
        }

        // ── Helpers ────────────────────────────────────────────────────────
        private async Task<List<DocumentChunk>> GetChunks(string fileId, string studentUserId, int limit)
            => await _mongo.Chunks
                .Find(c => c.FileId == fileId && c.StudentUserId == studentUserId)
                .SortBy(c => c.ChunkIndex).Limit(limit).ToListAsync();

        private static string Join(List<DocumentChunk> chunks)
            => string.Join("\n", chunks.Select(c => c.Content));
    }
}
