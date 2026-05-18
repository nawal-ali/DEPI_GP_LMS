using LMSProject.AI.Models;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;

namespace LMSProject.AI.Services
{
    /// <summary>
    /// Registered as Singleton in Program.cs.
    /// Provides typed collections for all AI/RAG data.
    /// </summary>
    public class MongoDbService
    {
        private readonly IMongoDatabase _db;

        public MongoDbService(IConfiguration cfg)
        {
            var connStr = cfg["MongoDB:ConnectionString"]
                ?? throw new InvalidOperationException("MongoDB:ConnectionString not configured.");
            var dbName = cfg["MongoDB:DatabaseName"] ?? "TOTC_LMS_AI";

            var client = new MongoClient(connStr);
            _db = client.GetDatabase(dbName);

            EnsureIndexes();
        }

        public IMongoCollection<DocumentChunk> Chunks
            => _db.GetCollection<DocumentChunk>("document_chunks");

        public IMongoCollection<UploadedFile> Files
            => _db.GetCollection<UploadedFile>("uploaded_files");

        public IMongoCollection<ChatMessage> Messages
            => _db.GetCollection<ChatMessage>("chat_messages");

        public IMongoCollection<ChatSession> Sessions
            => _db.GetCollection<ChatSession>("chat_sessions");

        private void EnsureIndexes()
        {
            // Chunks: query by student + file fast
            Chunks.Indexes.CreateOne(new CreateIndexModel<DocumentChunk>(
                Builders<DocumentChunk>.IndexKeys
                    .Ascending(c => c.StudentUserId)
                    .Ascending(c => c.FileId)));

            // Messages: query by session + student fast
            Messages.Indexes.CreateOne(new CreateIndexModel<ChatMessage>(
                Builders<ChatMessage>.IndexKeys
                    .Ascending(m => m.SessionId)
                    .Ascending(m => m.StudentUserId)));

            // Sessions: query by student fast
            Sessions.Indexes.CreateOne(new CreateIndexModel<ChatSession>(
                Builders<ChatSession>.IndexKeys
                    .Ascending(s => s.StudentUserId)));
        }
    }
}