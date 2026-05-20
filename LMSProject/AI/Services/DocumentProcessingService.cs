using DocumentFormat.OpenXml.Packaging;
using LMSProject.AI.Models;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using System.Text.RegularExpressions;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;
using System.Text.RegularExpressions;

namespace LMSProject.AI.Services
{
    public class DocumentProcessingService
    {
        private readonly MongoDbService _mongo;
        private readonly IWebHostEnvironment _env;
        private readonly int _chunkSize;
        private readonly int _overlap;

        private static readonly HashSet<string> AllowedExt
            = new(StringComparer.OrdinalIgnoreCase)
              { ".pdf", ".doc", ".docx", ".jpg", ".jpeg", ".png" };

        private const long MaxBytes = 20 * 1024 * 1024; // 20 MB

        public DocumentProcessingService(MongoDbService mongo,
            IConfiguration cfg, IWebHostEnvironment env)
        {
            _mongo = mongo;
            _env = env;
            _chunkSize = int.Parse(cfg["AI:ChunkSize"] ?? "800");
            _overlap = int.Parse(cfg["AI:ChunkOverlap"] ?? "100");
        }

        // ── Upload + process (fast — no embedding API calls) ───────────────
        public async Task<(bool ok, string message, UploadedFile? file)> ProcessUploadAsync(
            IFormFile upload, string studentUserId)
        {
            var ext = Path.GetExtension(upload.FileName);

            if (!AllowedExt.Contains(ext))
                return (false, $"File type '{ext}' not supported. Use PDF, DOCX, JPG, or PNG.", null);

            if (upload.Length > MaxBytes)
                return (false, "File exceeds 20 MB limit.", null);

            // Duplicate check
            var existing = await _mongo.Files
                .Find(f => f.StudentUserId == studentUserId && f.OriginalName == upload.FileName)
                .FirstOrDefaultAsync();
            if (existing != null)
                return (false, "A file with this name already exists. Rename it or delete the old one.", null);

            // Save to disk — ensure parent directories exist
            var aiRoot = Path.Combine(_env.WebRootPath, "Uploads", "AI");
            var folder = Path.Combine(aiRoot, studentUserId);
            Directory.CreateDirectory(aiRoot);
            Directory.CreateDirectory(folder);
            var storedName = $"{Guid.NewGuid()}{ext}";
            var fullPath = Path.Combine(folder, storedName);

            await using (var fs = System.IO.File.Create(fullPath))
                await upload.CopyToAsync(fs);

            var fileRecord = new UploadedFile
            {
                StudentUserId = studentUserId,
                OriginalName = upload.FileName,
                StoredPath = $"Uploads/AI/{studentUserId}/{storedName}",
                MimeType = upload.ContentType,
                FileSizeBytes = upload.Length,
                IsProcessed = false
            };
            await _mongo.Files.InsertOneAsync(fileRecord);

            // Extract text
            string text;
            try
            {
                text = ext.ToLower() switch
                {
                    ".pdf" => ExtractPdf(fullPath),
                    ".docx" => ExtractDocx(fullPath),
                    ".doc" => "[DOC format: please convert to DOCX for text extraction]",
                    _ => $"[Image file: {upload.FileName} — describe what you see in your question]"
                };
            }
            catch (Exception ex)
            {
                text = $"[Text extraction failed: {ex.Message}]";
            }

            // If extracted text is empty → scanned/image PDF
            //var cleanLen = text.Replace(" ", "").Replace("\r", "").Replace("\n", "").Length;
            //bool isScanned = cleanLen < 30;
            // Better scanned PDF detection
            var cleanedText = CleanText(text);

            bool isScanned =
                string.IsNullOrWhiteSpace(cleanedText)
                || cleanedText.Length < 10;
            if (isScanned)
            {
                text = $"[This file ({upload.FileName}) appears to be a scanned image-based PDF. " +
                       "No selectable text was found. Use a text-based PDF for AI features.]";
            }

            // Chunk text
            var chunks = ChunkText(text, fileRecord.Id, studentUserId, upload.FileName);
            if (chunks.Any())
                await _mongo.Chunks.InsertManyAsync(chunks);

            // Mark processed
            var upd = Builders<UploadedFile>.Update
                .Set(f => f.IsProcessed, true)
                .Set(f => f.ChunkCount, chunks.Count);
            await _mongo.Files.UpdateOneAsync(f => f.Id == fileRecord.Id, upd);

            fileRecord.IsProcessed = true;
            fileRecord.ChunkCount = chunks.Count;

            var resultMsg = isScanned
                ? $"⚠️ \"{upload.FileName}\" — no text found. This PDF is likely scanned/image-based."
                : $"✅ \"{upload.FileName}\" ready — {chunks.Count} sections indexed.";

            return (true, resultMsg, fileRecord);
        }

        // ── Retrieve relevant chunks — keyword matching fallback ───────────
        /// <summary>
        /// Fast keyword-based retrieval. No embedding API needed.
        /// Scores each chunk by how many query words it contains.
        /// </summary>
        public async Task<List<DocumentChunk>> RetrieveAsync(
            string query, string studentUserId, string? fileId = null, int topK = 5)
        {
            var filter = fileId != null
                ? Builders<DocumentChunk>.Filter.And(
                    Builders<DocumentChunk>.Filter.Eq(c => c.StudentUserId, studentUserId),
                    Builders<DocumentChunk>.Filter.Eq(c => c.FileId, fileId))
                : Builders<DocumentChunk>.Filter.Eq(c => c.StudentUserId, studentUserId);

            var allChunks = await _mongo.Chunks.Find(filter).ToListAsync();
            if (!allChunks.Any()) return new();

            // Keyword scoring
            var words = query.ToLower()
                .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .Where(w => w.Length > 2)
                .ToHashSet();

            return allChunks
                .Select(c => new
                {
                    Chunk = c,
                    Score = words.Count(w => c.Content.Contains(w, StringComparison.OrdinalIgnoreCase))
                })
                .Where(x => x.Score > 0)
                .OrderByDescending(x => x.Score)
                .Take(topK)
                .Select(x => x.Chunk)
                .ToList()
                // If no keyword matches, return first N chunks (still useful context)
                .DefaultIfEmpty() is var result && result.Any()
                    ? result.ToList()
                    : allChunks.Take(topK).ToList();
        }

        // ── Delete file + chunks ───────────────────────────────────────────
        public async Task DeleteFileAsync(string fileId, string studentUserId)
        {
            var file = await _mongo.Files
                .Find(f => f.Id == fileId && f.StudentUserId == studentUserId)
                .FirstOrDefaultAsync();
            if (file == null) return;

            var fullPath = Path.Combine(_env.WebRootPath, file.StoredPath);
            if (System.IO.File.Exists(fullPath))
                System.IO.File.Delete(fullPath);

            await _mongo.Chunks.DeleteManyAsync(c => c.FileId == fileId);
            await _mongo.Files.DeleteOneAsync(f => f.Id == fileId);
        }

        // ── Get single file record ────────────────────────────────────────
        public async Task<UploadedFile?> GetFileRecordAsync(string fileId, string userId)
            => await _mongo.Files
                .Find(f => f.Id == fileId && f.StudentUserId == userId)
                .FirstOrDefaultAsync();

        // ── Get student files list ─────────────────────────────────────────
        public async Task<List<UploadedFile>> GetStudentFilesAsync(string studentUserId)
            => await _mongo.Files
                .Find(f => f.StudentUserId == studentUserId)
                .SortByDescending(f => f.UploadedAt)
                .ToListAsync();

        // ── Private helpers ────────────────────────────────────────────────
        private static string ExtractPdf(string path)
        {
            using var pdf = PdfDocument.Open(path);

            var sb = new System.Text.StringBuilder();

            foreach (Page page in pdf.GetPages())
            {
                try
                {
                    // ===== Strategy 1: page.Text =====
                    var pageText = page.Text ?? "";

                    pageText = CleanText(pageText);

                    if (!string.IsNullOrWhiteSpace(pageText) && pageText.Length > 10)
                    {
                        sb.AppendLine(pageText);
                        continue;
                    }

                    // ===== Strategy 2: GetWords() =====
                    try
                    {
                        var words = page.GetWords();

                        var wordText = string.Join(" ",
                            words.Select(w => w.Text));

                        wordText = CleanText(wordText);

                        if (!string.IsNullOrWhiteSpace(wordText) && wordText.Length > 10)
                        {
                            sb.AppendLine(wordText);
                            continue;
                        }
                    }
                    catch { }

                    // ===== Strategy 3: Raw letters =====
                    try
                    {
                        var letters = page.Letters;

                        if (letters != null && letters.Any())
                        {
                            var raw = string.Concat(
                                letters.Select(l => l.Value));

                            raw = CleanText(raw);

                            if (!string.IsNullOrWhiteSpace(raw) && raw.Length > 10)
                            {
                                sb.AppendLine(raw);
                            }
                        }
                    }
                    catch { }
                }
                catch { }
            }

            return CleanText(sb.ToString());
        }

        private static string CleanText(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return string.Empty;

            // Remove invisible/control chars
            text = Regex.Replace(text, @"\p{C}+", " ");

            // Normalize spaces
            text = Regex.Replace(text, @"\s+", " ");

            return text.Trim();
        }

        private static string ExtractDocx(string path)
        {
            using var doc = WordprocessingDocument.Open(path, false);
            return doc.MainDocumentPart?.Document.Body?.InnerText ?? "";
        }

        private List<DocumentChunk> ChunkText(string text, string fileId,
            string studentUserId, string fileName)
        {
            var chunks = new List<DocumentChunk>();
            if (string.IsNullOrWhiteSpace(text)) return chunks;

            int start = 0, index = 0;
            while (start < text.Length)
            {
                int end = Math.Min(start + _chunkSize, text.Length);
                var content = text[start..end].Trim();
                if (!string.IsNullOrEmpty(content))
                    chunks.Add(new DocumentChunk
                    {
                        StudentUserId = studentUserId,
                        FileId = fileId,
                        FileName = fileName,
                        ChunkIndex = index++,
                        Content = content,
                        Embedding = Array.Empty<float>() // skipped — see class summary
                    });
                start = end - _overlap;
                if (start >= end) break;
            }
            return chunks;
        }
    }
}