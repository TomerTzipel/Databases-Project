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
            {
                // Fallback to repo-local path for grading
                var repoRoot = AppContext.BaseDirectory;
                // climb out of bin/Debug/netX.Y/ to the repo root
                for (int i = 0; i < 5; i++) repoRoot = Path.GetDirectoryName(repoRoot)!;
                var localKey = Path.Combine(repoRoot, "secrets", "firebase-analytics.json");
                if (File.Exists(localKey))
                    credPath = localKey;
            }

            if (string.IsNullOrWhiteSpace(credPath) || !File.Exists(credPath))
                throw new Exception("Firebase key not found. Set GOOGLE_APPLICATION_CREDENTIALS or place secrets/firebase-analytics.json.");

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
            // derive metrics with no per-question arrays
            float avgAnswerTime = dto.NumQuestions > 0 ? dto.DurationSeconds / dto.NumQuestions : 0f;
            float correctRate = dto.NumQuestions > 0 ? (float)dto.NumCorrect / dto.NumQuestions : 0f;

            // 1) write per-match doc
            var matchDoc = new
            {
                matchId = string.IsNullOrWhiteSpace(dto.MatchId) ? Guid.NewGuid().ToString() : dto.MatchId,
                playerId = dto.PlayerId,
                playerName = dto.PlayerName,
                score = dto.Score,
                didWin = dto.DidWin,
                numCorrect = dto.NumCorrect,
                numQuestions = dto.NumQuestions,
                durationSeconds = dto.DurationSeconds,
                avgAnswerTime,
                correctRate,
                createdAt = Timestamp.GetCurrentTimestamp()
            };
            await _db.Collection("analytics_matches").AddAsync(matchDoc);

            // 2) update per-player aggregates
            var playerRef = _db.Collection("analytics_players").Document(dto.PlayerId);
            await _db.RunTransactionAsync(async tx =>
            {
                var snap = await tx.GetSnapshotAsync(playerRef);

                long gamesPlayed = snap.Exists ? snap.GetValue<long>("gamesPlayed") : 0;
                long gamesWon = snap.Exists ? snap.GetValue<long>("gamesWon") : 0;
                double totalTime = snap.Exists ? Convert.ToDouble(snap.GetValue<double>("totalAnswerTime")) : 0d;
                long totalQ = snap.Exists ? snap.GetValue<long>("totalQuestions") : 0;
                long totalCorrect = snap.Exists ? snap.GetValue<long>("totalCorrect") : 0;
                long totalScore = snap.Exists ? snap.GetValue<long>("totalScore") : 0;
                long highScore = snap.Exists ? snap.GetValue<long>("highScore") : 0;

                gamesPlayed += 1;
                if (dto.DidWin) gamesWon += 1;
                totalTime += dto.DurationSeconds;
                totalQ += dto.NumQuestions;
                totalCorrect += dto.NumCorrect;
                totalScore += dto.Score;
                highScore = Math.Max(highScore, dto.Score);

                var updates = new Dictionary<string, object>
                {
                    ["playerId"] = dto.PlayerId,
                    ["name"] = dto.PlayerName,
                    ["gamesPlayed"] = gamesPlayed,
                    ["gamesWon"] = gamesWon,
                    ["winRate"] = gamesPlayed > 0 ? (double)gamesWon / gamesPlayed : 0.0,
                    ["totalAnswerTime"] = totalTime,
                    ["totalQuestions"] = totalQ,
                    ["totalCorrect"] = totalCorrect,
                    ["avgAnswerTime"] = totalQ > 0 ? totalTime / totalQ : 0.0,
                    ["totalScore"] = totalScore,
                    ["highScore"] = highScore,
                    ["lastMatchAt"] = Timestamp.GetCurrentTimestamp()
                };

                tx.Set(playerRef, updates, SetOptions.MergeAll);
            });
        }
    }
}
