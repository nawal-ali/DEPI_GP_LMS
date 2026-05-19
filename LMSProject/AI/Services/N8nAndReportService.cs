using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using MLSEF;

namespace LMSProject.AI.Services
{
    public class N8nService
    {
        private readonly HttpClient _http;
        private readonly string _webhookBase;
        private readonly string _apiKey;

        public N8nService(IHttpClientFactory factory, IConfiguration cfg)
        {
            _http = factory.CreateClient("N8N");
            _webhookBase = cfg["N8N:WebhookBaseUrl"] ?? "PLACEHOLDER_N8N_WEBHOOK_URL";
            _apiKey = cfg["N8N:ApiKey"] ?? "PLACEHOLDER_N8N_API_KEY";
        }

        public async Task TriggerAsync(string webhookPath, object payload)
        {
            var url = $"{_webhookBase.TrimEnd('/')}/{webhookPath.TrimStart('/')}";
            var body = new StringContent(
                JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

            _http.DefaultRequestHeaders.Remove("X-N8N-API-Key");
            _http.DefaultRequestHeaders.Add("X-N8N-API-Key", _apiKey);

            var resp = await _http.PostAsync(url, body);
            resp.EnsureSuccessStatusCode();
        }
    }

    public class WeeklyReportService
    {
        private readonly AppDbContext _db;
        private readonly EmailService _email;
        private readonly N8nService _n8n;
        private readonly GithubAiService _ai;

        public WeeklyReportService(AppDbContext db, EmailService email,
            N8nService n8n, GithubAiService ai)
        { _db = db; _email = email; _n8n = n8n; _ai = ai; }

        public async Task SendAllReportsAsync()
        {
            var parents = await _db.Parents
                .Include(p => p.Children)
                .ThenInclude(c => c.User)
                .Where(p => !string.IsNullOrEmpty(p.Email))
                .ToListAsync();

            foreach (var parent in parents)
            {
                foreach (var student in parent.Children.Where(c => c.CurrentState == 1))
                {
                    try
                    {
                        var html = await BuildReportHtmlAsync(student.Id);
                        await _email.SendWeeklyReportAsync(
                            parent.Email!, parent.FullName, student.FullName, html);

                        await _n8n.TriggerAsync("weekly-report-sent", new
                        {
                            parentEmail = parent.Email,
                            studentName = student.FullName,
                            sentAt = DateTime.UtcNow
                        });
                    }
                    catch { /* log & continue */ }
                }
            }
        }

        private async Task<string> BuildReportHtmlAsync(int studentId)
        {
            var now = DateTime.Now;
            var weekAgo = now.AddDays(-7);

            // Course IDs the student is enrolled in
            var courseIds = await _db.StudentCourses
                .Where(sc => sc.StId == studentId)
                .Select(sc => sc.CourseId)
                .ToListAsync();

            // Build a course-name lookup dictionary to avoid .Course navigation errors
            var courseNames = await _db.Courses
                .Where(c => courseIds.Contains(c.Id))
                .ToDictionaryAsync(c => c.Id, c => c.Name);

            // Exam results this week — NO Include(t => t.Course), use dictionary instead
            var examResults = await _db.StudentTests
                .Where(st => st.StudentId == studentId && st.JoinDate >= weekAgo)
                .Include(st => st.Test)
                .ToListAsync();

            // Assignments — NO Include(a => a.Course), use dictionary instead
            var assignments = await _db.Assignments
                .Where(a => courseIds.Contains(a.CourseId) && a.CurrentState == 1)
                .Include(a => a.Submissions.Where(s => s.StudentId == studentId))
                .ToListAsync();

            // Upcoming exams not yet taken
            var takenIds = examResults.Select(e => e.TestId).ToHashSet();

            var upcomingExams = await _db.Tests
                .Where(t => courseIds.Contains(t.CourseId) && t.CurrentState == 1
                         && t.Deadline.HasValue && t.Deadline > now
                         && !takenIds.Contains(t.Id))
                .ToListAsync();

            var submitted = assignments.Where(a => a.Submissions.Any()).ToList();
            var missing = assignments.Where(a => !a.Submissions.Any() && a.Deadline < now).ToList();

            double avg = examResults.Any()
                ? examResults.Average(e => e.Test?.TotalMarks > 0
                    ? e.Score / e.Test.TotalMarks * 100 : 0)
                : 0;

            var sb = new StringBuilder();

            // Exam results
            if (examResults.Any())
            {
                sb.Append("<h4 style='color:#2F327D;margin-top:0;'>📝 Exam Results This Week</h4>");
                sb.Append($"<p>Average score: <strong>{avg:F1}%</strong></p>");
                sb.Append("<table style='width:100%;border-collapse:collapse;font-size:.85rem;margin-bottom:1rem;'>");
                sb.Append("<tr style='background:#f4f6fb;'><th style='padding:8px;text-align:left;'>Exam</th><th>Score</th><th>Date</th></tr>");

                foreach (var r in examResults)
                {
                    double pct = r.Test?.TotalMarks > 0 ? r.Score / r.Test.TotalMarks * 100 : 0;
                    sb.Append($"<tr><td style='padding:8px;border-bottom:1px solid #eee;'>{r.Test?.Title ?? "—"}</td>");
                    sb.Append($"<td style='padding:8px;border-bottom:1px solid #eee;'>{r.Score}/{r.Test?.TotalMarks} ({pct:F0}%)</td>");
                    sb.Append($"<td style='padding:8px;border-bottom:1px solid #eee;'>{r.JoinDate:MMM dd}</td></tr>");
                }
                sb.Append("</table>");
            }
            else
            {
                sb.Append("<p>No exams taken this week.</p>");
            }

            // Assignment summary
            sb.Append("<h4 style='color:#2F327D;'>📋 Assignment Summary</h4>");
            sb.Append($"<p>✅ Submitted: <strong>{submitted.Count}</strong> &nbsp; " +
                      $"⚠️ Missing: <strong style='color:#E13468;'>{missing.Count}</strong></p>");

            if (missing.Any())
            {
                sb.Append("<p style='color:#E13468;font-weight:600;'>Missing Assignments:</p><ul>");
                foreach (var a in missing.Take(5))
                {
                    var cname = courseNames.GetValueOrDefault(a.CourseId, "Unknown Course");
                    sb.Append($"<li>{a.Title} ({cname}) — was due {a.Deadline:MMM dd}</li>");
                }
                sb.Append("</ul>");
            }

            // Upcoming exams
            if (upcomingExams.Any())
            {
                sb.Append("<h4 style='color:#2F327D;'>📅 Upcoming Exams</h4><ul>");
                foreach (var e in upcomingExams.Take(5))
                {
                    var cname = courseNames.GetValueOrDefault(e.CourseId, "Unknown Course");
                    sb.Append($"<li>{e.Title} ({cname}) — {e.Deadline:MMM dd, yyyy}</li>");
                }
                sb.Append("</ul>");
            }

            // AI narrative analysis
            try
            {
                var aiPrompt =
                    $"Write a 3-sentence plain-language weekly academic summary for a parent.\n" +
                    $"Student average: {avg:F1}%\n" +
                    $"Exams taken this week: {examResults.Count}\n" +
                    $"Missing assignments: {missing.Count}\n" +
                    $"Submitted assignments: {submitted.Count}\n\n" +
                    "Be warm, honest and direct. No bullet points. Plain sentences only.";

                var narrative = await _ai.ChatAsync(
                    new List<(string, string)> { ("user", aiPrompt) },
                    "You are a school assistant writing parent summaries. Be warm and factual.");

                sb.Append("<div style='background:#f8f9ff;border-left:4px solid #5B72EE;" +
                          "border-radius:0 12px 12px 0;padding:1rem 1.25rem;margin-top:1rem;'>" +
                          "<strong style='color:#2F327D;'>📊 AI Analysis</strong><br/>" +
                          $"<span style='font-size:.9rem;color:#374151;line-height:1.8;'>{System.Net.WebUtility.HtmlEncode(narrative)}</span>" +
                          "</div>");
            }
            catch { /* AI optional — don't block email on failure */ }

            return sb.ToString();
        }
    }
}