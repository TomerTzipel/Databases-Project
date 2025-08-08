using Google.Apis.Auth.OAuth2;
using Google.Cloud.Firestore;
using System.Text.Json;

namespace TriviaServer.Services
{
    public class FirestoreAnalytics
    {
        private static FirestoreAnalytics? _instance;
        private static readonly object _lock = new();
        private readonly FirestoreDb _db;

        private FirestoreAnalytics()
        {
            var credPath = Environment.GetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS");
            if (string.IsNullOrWhiteSpace(credPath) || !File.Exists(credPath))
                throw new Exception("GOOGLE_APPLICATION_CREDENTIALS must point to the Firebase service-account JSON file.");

            var saJson = File.ReadAllText(credPath);
            using var doc = JsonDocument.Parse(saJson);
            var projectId = doc.RootElement.GetProperty("project_id").GetString()!;

            var builder = new FirestoreDbBuilder
            {
                ProjectId = projectId,
                Credential = GoogleCredential.FromFile(credPath)
            };
            _db = builder.Build();
        }

        public static FirestoreAnalytics Instance
        {
            get { lock (_lock) return _instance ??= new FirestoreAnalytics(); }
        }

        public async Task LogGameOver(Models.GameOverDto dto)
        {
            await _db.Collection("analytics_matches").AddAsync(new
            {
                matchId = string.IsNullOrWhiteSpace(dto.MatchId) ? Guid.NewGuid().ToString() : dto.MatchId,
                playerId = dto.PlayerId,
                score = dto.Score,
                durationSeconds = dto.DurationSeconds,
                questionIds = dto.QuestionIds ?? new List<string>(),
                answers = dto.Answers ?? new List<int>(),
                correctIndexes = dto.CorrectIndexes ?? new List<int>(),
                mode = dto.Mode,
                build = dto.Build,
                createdAt = Timestamp.GetCurrentTimestamp()
            });
        }
    }
}
