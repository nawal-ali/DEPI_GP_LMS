using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Text.Json;

namespace LMSProject.Controllers
{
    [Route("api/ai")]
    public class AiProxyController : ControllerBase
    {
        private readonly IConfiguration _cfg;
        private readonly IHttpClientFactory _http;

        public AiProxyController(IConfiguration cfg, IHttpClientFactory http)
        { _cfg = cfg; _http = http; }

        [HttpPost("chat")]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> Chat(IFormFile? file, [FromForm] string question)
        {
            if (string.IsNullOrWhiteSpace(question))
                return BadRequest("Question is required.");
            var url = _cfg["N8N:ChatbotWebhook"];
            if (string.IsNullOrEmpty(url)) return BadRequest("N8N:ChatbotWebhook not set.");
            return await Forward(url, question, file);
        }

        [HttpPost("generate-exam")]
        [Authorize(Roles = "Instructor")]
        public async Task<IActionResult> GenerateExam(
            IFormFile? file, [FromForm] int mcqCount = 5, [FromForm] int tfCount = 3)
        {
            if (file == null || file.Length == 0) return BadRequest("No file provided.");
            var url = _cfg["N8N:ExamGeneratorWebhook"];
            if (string.IsNullOrEmpty(url)) return BadRequest("N8N:ExamGeneratorWebhook not set.");

            var prompt =
                $"Generate an exam from the document.\n" +
                $"- {mcqCount} MCQ questions (A/B/C/D, mark answer)\n" +
                $"- {tfCount} True/False statements\n" +
                $"Format:\n=== MCQ ===\n1. Q\n   A)...\n   Answer: X\n\n=== True/False ===\n1. Statement -> True/False";
            return await Forward(url, prompt, file);
        }

        private async Task<IActionResult> Forward(string url, string question, IFormFile? file)
        {
            try
            {
                var client = _http.CreateClient();
                client.Timeout = TimeSpan.FromSeconds(120);

                using var form = new MultipartFormDataContent();
                form.Add(new StringContent(question), "question");

                if (file != null && file.Length > 0)
                {
                    var ms = new MemoryStream();
                    await file.CopyToAsync(ms);
                    ms.Position = 0;
                    var fc = new StreamContent(ms);
                    fc.Headers.ContentType = new MediaTypeHeaderValue(
                        string.IsNullOrEmpty(file.ContentType) ? "application/octet-stream" : file.ContentType);
                    fc.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data")
                    {
                        Name = "\"file\"",
                        FileName = "\"" + file.FileName + "\""
                    };
                    form.Add(fc);
                }

                Console.WriteLine($"[AI Proxy] Calling n8n URL: {url}");

                var resp = await client.PostAsync(url, form);
                var raw = await resp.Content.ReadAsStringAsync() ?? "";

                Console.WriteLine($"[AI Proxy] n8n Status: {(int)resp.StatusCode} {resp.StatusCode}");
                Console.WriteLine($"[AI Proxy] n8n Body (first 300): {raw[..Math.Min(300, raw.Length)]}");

                // n8n returned HTML error page → test webhook not active or wrong URL
                if (raw.TrimStart().StartsWith("<"))
                    return StatusCode(502,
                        $"n8n error: workflow not Published or wrong URL. HTTP {(int)resp.StatusCode}. Open n8n, click Publish, use production webhook URL.");

                if (!resp.IsSuccessStatusCode)
                    return StatusCode((int)resp.StatusCode,
                        string.IsNullOrWhiteSpace(raw)
                            ? $"n8n returned empty body with status {(int)resp.StatusCode}. Is the workflow Published?"
                            : raw);

                if (string.IsNullOrWhiteSpace(raw))
                    return StatusCode(502,
                        "n8n returned an empty response. Check the 'Respond to Webhook' node in your workflow.");

                var text = ExtractText(raw);
                return Content(text, "text/plain; charset=utf-8");
            }
            catch (TaskCanceledException) { return StatusCode(504, "AI request timed out."); }
            catch (Exception ex) { return StatusCode(500, ex.Message); }
        }

        private static string ExtractText(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return "";
            try
            {
                using var doc = JsonDocument.Parse(raw);
                var root = doc.RootElement;
                var fields = new[] { "output", "text", "answer", "result", "message", "content", "response" };

                // n8n returns array: [{ "output": "..." }]
                if (root.ValueKind == JsonValueKind.Array && root.GetArrayLength() > 0)
                {
                    var first = root[0];
                    foreach (var f in fields)
                        if (first.TryGetProperty(f, out var v) && v.ValueKind == JsonValueKind.String)
                            return v.GetString()!;
                }

                // n8n returns object: { "output": "..." }
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
    }
}