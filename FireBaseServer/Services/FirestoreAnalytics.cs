using System;
using System.IO;
using System.Text.Json;
using System.Collections.Generic;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Firestore;
using Google.Cloud.Firestore.V1;
using TriviaServer.Models;

namespace TriviaServer.Services
{
    public enum MatchResult
    {
        Win, Lose, Tie
    }
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
                var dir = AppContext.BaseDirectory;
                for (int i = 0; i < 5; i++) dir = Directory.GetParent(dir)!.FullName;
                var fallback = Path.Combine(dir, "secrets", "firebase-analytics.json");
                if (File.Exists(fallback)) credPath = fallback;
            }
            if (string.IsNullOrWhiteSpace(credPath) || !File.Exists(credPath))
                throw new Exception("Firebase key not found. Set GOOGLE_APPLICATION_CREDENTIALS or place secrets/firebase-analytics.json");

            var saJson = File.ReadAllText(credPath);
            using var doc = JsonDocument.Parse(saJson);
            var projectId = doc.RootElement.GetProperty("project_id").GetString()!;

            var credential = GoogleCredential.FromFile(credPath);
            if (credential.IsCreateScopedRequired)
                credential = credential.CreateScoped(FirestoreClient.DefaultScopes);

            Console.WriteLine($"[Firestore] Using key: {credPath}");
            Console.WriteLine($"[Firestore] Project: {projectId}");

            _db = new FirestoreDbBuilder
            {
                ProjectId = projectId,
                Credential = credential
            }.Build();
        }

        public static FirestoreAnalytics Instance
        {
            get { lock (_lock) return _instance ??= new FirestoreAnalytics(); }
        }
        public async Task <PlayerAnalytics?> GetPlayerAnalytics(string playerName)
        {
            if (string.IsNullOrWhiteSpace(playerName))
                return null;
            var playerRef = _db.Collection("analytics_players").Document(playerName);
            var snapshot = await playerRef.GetSnapshotAsync();
            if (!snapshot.Exists)
                return null;
            return new PlayerAnalytics
            {
                 TotalGamesPlayed = snapshot.GetValue<long>("gamesPlayed"),
                 TotalWins =  snapshot.GetValue<long>("gamesWon"),
                 TotalLosses = snapshot.GetValue<long>("gamesLost"),
                 TotalTies = snapshot.GetValue<long>("gamesTied"),
                 AverageDurationSeconds = snapshot.GetValue<double>("avgMatchTime"),
               
            };
        }
        public async Task LogGameOver(TriviaServer.Models.GameOverDto dto)
        {
            // ---- ONLY per-player aggregate (doc id = playerName). No match docs. ----
            var playerRef = _db.Collection("analytics_players").Document(dto.PlayerName);

            await _db.RunTransactionAsync(async tx =>
            {
                var s = await tx.GetSnapshotAsync(playerRef);

                long gamesPlayed = s.Exists ? s.GetValue<long>("gamesPlayed") : 0;
                long gamesWon = s.Exists ? s.GetValue<long>("gamesWon") : 0;
                long gamesLost = s.Exists ? s.GetValue<long>("gamesLost") : 0;
                long gamesTied = s.Exists ? s.GetValue<long>("gamesTied") : 0;
                double totalTime = s.Exists ? Convert.ToDouble(s.GetValue<double>("totalMatchTime")) : 0d;
                long totalCorrect = s.Exists ? s.GetValue<long>("totalCorrect") : 0;

                gamesPlayed += 1;
                MatchResult outcome = (MatchResult)dto.Outcome;
                switch (outcome)
                {
                    case MatchResult.Win: 
                        gamesWon += 1;
                        break;
                    case MatchResult.Lose:
                        gamesLost += 1;
                        break;
                    case MatchResult.Tie: 
                        gamesTied += 1;
                        break;
                    default:
                        break;
                }

                totalTime += dto.DurationSeconds;
                totalCorrect += dto.NumCorrect;

                var updates = new Dictionary<string, object>
                {
                    ["name"] = dto.PlayerName,
                    ["gamesPlayed"] = gamesPlayed,
                    ["gamesWon"] = gamesWon,
                    ["gamesLost"] = gamesLost,
                    ["gamesTied"] = gamesTied,
                    ["winRate"] = gamesPlayed > 0 ? (double)gamesWon / gamesPlayed : 0.0,
                    ["totalMatchTime"] = totalTime,
                    ["avgMatchTime"] = gamesPlayed > 0 ? totalTime / gamesPlayed : 0.0,
                    ["totalCorrect"] = totalCorrect,
                    ["lastMatchAt"] = Timestamp.GetCurrentTimestamp()
                };

                tx.Set(playerRef, updates, SetOptions.MergeAll);
            });
        }
    }
}
