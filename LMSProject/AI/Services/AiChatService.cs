using LMSProject.AI.Models;
using MongoDB.Driver;
using MLSEF;
using Microsoft.EntityFrameworkCore;

namespace LMSProject.AI.Services
{
    public class AiChatService
    {
        private readonly MongoDbService _mongo;
        private readonly GithubAiService _ai;
        private readonly DocumentProcessingService _docs;
        private readonly AppDbContext _db;

        public AiChatService(MongoDbService mongo, GithubAiService ai,
            DocumentProcessingService docs, AppDbContext db)
        { _mongo = mongo; _ai = ai; _docs = docs; _db = db; }

        // ── RAG Chat ───────────────────────────────────────────────────────
        public async Task<string> ChatAsync(string question, string sessionId,
            string studentUserId, string? fileId = null)
        {
            var chunks = await _docs.RetrieveAsync(question, studentUserId, fileId, topK: 5);
            var context = chunks.Any()
                ? string.Join("\n\n---\n\n", chunks.Select(c => c.Content))
                : "No specific document context available.";

            var system = "You are a helpful AI study assistant for students. " +
                         "Answer ONLY based on the provided context. " +
                         "If the answer is not in the context, say so clearly. " +
                         "Be concise, clear, and educational.";

            // Recent history (last 6 messages)
            var history = await _mongo.Messages
                .Find(m => m.SessionId == sessionId)
                .SortByDescending(m => m.CreatedAt)
                .Limit(6)
                .ToListAsync();

            history.Reverse();

            var messages = history
                .Select(m => (m.Role, m.Content))
                .Append(("user", $"Context:\n{context}\n\nQuestion: {question}"))
                .ToList();

            var answer = await _ai.ChatAsync(messages, system);

            await _mongo.Messages.InsertManyAsync(new[]
            {
                new ChatMessage { SessionId=sessionId, StudentUserId=studentUserId,
                                  Role="user",      Content=question, SourceFileId=fileId },
                new ChatMessage { SessionId=sessionId, StudentUserId=studentUserId,
                                  Role="assistant", Content=answer,   SourceFileId=fileId }
            });

            await _mongo.Sessions.UpdateOneAsync(
                s => s.Id == sessionId,
                Builders<ChatSession>.Update.Set(s => s.UpdatedAt, DateTime.UtcNow));

            return answer;
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

        // ── AI Tools ───────────────────────────────────────────────────────
        public async Task<string> SummarizeAsync(string fileId, string studentUserId)
        {
            var chunks = await GetChunks(fileId, studentUserId, 10);
            if (!chunks.Any()) return "No content found for this file.";
            var text = Join(chunks);
            return await _ai.ChatAsync(
                new List<(string, string)> { ("user", $"Summarize this document:\n{text}") },
                "You are an expert summarizer. Provide a clear, structured summary.");
        }

        public async Task<string> GenerateMcqAsync(string fileId, string studentUserId, int count = 5)
        {
            var chunks = await GetChunks(fileId, studentUserId, 8);
            if (!chunks.Any()) return "No content found.";
            var text = Join(chunks);
            return await _ai.ChatAsync(
                new List<(string, string)>
                {
                    ("user", $"Generate {count} MCQs from this text. Format each:\nQ: ...\nA) ...\nB) ...\nC) ...\nD) ...\nCorrect: ...\n\nText:\n{text}")
                },
                "You are an expert teacher. Generate clear, educational MCQs.");
        }

        public async Task<string> KeyPointsAsync(string fileId, string studentUserId)
        {
            var chunks = await GetChunks(fileId, studentUserId, 8);
            if (!chunks.Any()) return "No content found.";
            var text = Join(chunks);
            return await _ai.ChatAsync(
                new List<(string, string)>
                {
                    ("user", $"Extract the 10 most important key points as a numbered list:\n{text}")
                },
                "You are an expert educator. Extract the most important concepts.");
        }

        // ── Study Planner ──────────────────────────────────────────────────
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

            // Use dictionary for course names — avoids .Course navigation issues
            var courseNames = await _db.Courses
                .Where(c => courseIds.Contains(c.Id))
                .ToDictionaryAsync(c => c.Id, c => c.Name);

            // Upcoming exams — do NOT Include(t => t.Course), use dictionary
            var exams = await _db.Tests
                .Where(t => courseIds.Contains(t.CourseId) && t.CurrentState == 1
                         && t.Deadline.HasValue && t.Deadline > now)
                .OrderBy(t => t.Deadline)
                .Take(10)
                .ToListAsync();

            // Pending assignments — do NOT Include(a => a.Course), use dictionary
            var assignments = await _db.Assignments
                .Where(a => courseIds.Contains(a.CourseId) && a.CurrentState == 1 && a.Deadline > now)
                .Include(a => a.Submissions.Where(s => s.StudentId == student.Id))
                .OrderBy(a => a.Deadline)
                .Take(10)
                .ToListAsync();

            var examLines = exams.Select(e =>
                $"- {e.Title} ({courseNames.GetValueOrDefault(e.CourseId, "Unknown")}) — Due {e.Deadline:MMM dd, yyyy}");

            var assignLines = assignments
                .Where(a => !a.Submissions.Any())
                .Select(a =>
                    $"- {a.Title} ({courseNames.GetValueOrDefault(a.CourseId, "Unknown")}) — Due {a.Deadline:MMM dd, yyyy}");

            var context =
                $"Student: {student.FullName}\n" +
                $"Grade: {student.Grade?.Name ?? "Unknown"}\n\n" +
                $"Upcoming Exams:\n{string.Join("\n", examLines)}\n\n" +
                $"Pending Assignments:\n{string.Join("\n", assignLines)}";

            return await _ai.ChatAsync(
                new List<(string, string)> { ("user", context) },
                "You are a professional academic advisor. " +
                "Based on the student's upcoming deadlines, create a personalized 7-day study plan. " +
                "Prioritize by urgency. Be specific about what to study each day. " +
                "Format as a clear day-by-day schedule.");
        }

        // ── Helpers ────────────────────────────────────────────────────────
        private async Task<List<DocumentChunk>> GetChunks(
            string fileId, string studentUserId, int limit)
            => await _mongo.Chunks
                .Find(c => c.FileId == fileId && c.StudentUserId == studentUserId)
                .SortBy(c => c.ChunkIndex)
                .Limit(limit)
                .ToListAsync();

        private static string Join(List<DocumentChunk> chunks)
            => string.Join("\n", chunks.Select(c => c.Content));
    }
}