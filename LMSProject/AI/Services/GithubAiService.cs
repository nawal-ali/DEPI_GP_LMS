using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;

namespace LMSProject.AI.Services
{
    /// <summary>
    /// Thin wrapper around the GitHub Models inference endpoint.
    /// GitHub Models uses the same JSON schema as the OpenAI REST API.
    ///
    /// API Key  → appsettings.json → AI:ApiKey
    /// Base URL → https://models.inference.ai.azure.com
    /// </summary>
    public class GithubAiService
    {
        private readonly HttpClient _http;
        private readonly string _chatModel;
        private readonly string _embedModel;
        private readonly int _maxTokens;

        public GithubAiService(IConfiguration cfg, IHttpClientFactory factory)
        {
            _http = factory.CreateClient("GithubAI");

            var apiKey = cfg["AI:ApiKey"] ?? "PLACEHOLDER_GITHUB_MODELS_API_KEY";
            var baseUrl = cfg["AI:BaseUrl"] ?? "https://models.inference.ai.azure.com";

            _chatModel = cfg["AI:ChatModel"] ?? "gpt-4o-mini";
            _embedModel = cfg["AI:EmbedModel"] ?? "text-embedding-3-small";
            _maxTokens = int.Parse(cfg["AI:MaxTokens"] ?? "1024");

            _http.BaseAddress = new Uri(baseUrl);
            _http.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", apiKey);
        }

        // ── Chat Completion ────────────────────────────────────────────────
        public async Task<string> ChatAsync(List<(string role, string content)> messages,
            string? systemPrompt = null)
        {
            var msgs = new List<object>();
            if (!string.IsNullOrEmpty(systemPrompt))
                msgs.Add(new { role = "system", content = systemPrompt });

            foreach (var (role, content) in messages)
                msgs.Add(new { role, content });

            var body = new
            {
                model = _chatModel,
                max_tokens = _maxTokens,
                messages = msgs
            };

            var resp = await _http.PostAsync("/chat/completions",
                new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json"));

            resp.EnsureSuccessStatusCode();
            var json = await resp.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            return doc.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString() ?? "";
        }

        // ── Embeddings ─────────────────────────────────────────────────────
        public async Task<float[]> EmbedAsync(string text)
        {
            var body = new { model = _embedModel, input = text };
            var resp = await _http.PostAsync("/embeddings",
                new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json"));

            resp.EnsureSuccessStatusCode();
            var json = await resp.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);

            var arr = doc.RootElement
                .GetProperty("data")[0]
                .GetProperty("embedding")
                .EnumerateArray()
                .Select(e => e.GetSingle())
                .ToArray();

            return arr;
        }

        // ── Helpers ────────────────────────────────────────────────────────
        /// <summary>Cosine similarity between two embedding vectors.</summary>
        public static float CosineSimilarity(float[] a, float[] b)
        {
            float dot = 0, magA = 0, magB = 0;
            for (int i = 0; i < Math.Min(a.Length, b.Length); i++)
            {
                dot += a[i] * b[i];
                magA += a[i] * a[i];
                magB += b[i] * b[i];
            }
            float denom = MathF.Sqrt(magA) * MathF.Sqrt(magB);
            return denom == 0 ? 0 : dot / denom;
        }
    }
}