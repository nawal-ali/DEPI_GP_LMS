using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace LMSProject.AI.Models
{
    /// <summary>A chunk of text extracted from a student-uploaded document.</summary>
    public class DocumentChunk
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

        public string StudentUserId { get; set; } = "";
        public string FileId { get; set; } = "";  // references UploadedFile.Id
        public string FileName { get; set; } = "";
        public int ChunkIndex { get; set; }
        public string Content { get; set; } = "";

        // Cosine similarity embedding stored as array of floats
        public float[] Embedding { get; set; } = Array.Empty<float>();

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    /// <summary>Metadata for a file uploaded by a student for RAG.</summary>
    public class UploadedFile
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

        public string StudentUserId { get; set; } = "";
        public string OriginalName { get; set; } = "";
        public string StoredPath { get; set; } = "";
        public string MimeType { get; set; } = "";
        public long FileSizeBytes { get; set; }
        public int ChunkCount { get; set; }
        public bool IsProcessed { get; set; }
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
    }

    /// <summary>Single message in a student's chat history.</summary>
    public class ChatMessage
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

        public string SessionId { get; set; } = "";
        public string StudentUserId { get; set; } = "";

        /// <summary>user | assistant</summary>
        public string Role { get; set; } = "user";
        public string Content { get; set; } = "";
        public string? SourceFileId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    /// <summary>A chat session groups messages and file context together.</summary>
    public class ChatSession
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

        public string StudentUserId { get; set; } = "";
        public string Title { get; set; } = "New Chat";
        public List<string> FileIds { get; set; } = new();
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}