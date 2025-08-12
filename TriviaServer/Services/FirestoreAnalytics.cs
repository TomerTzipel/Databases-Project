using Google.Apis.Auth.OAuth2;
using Google.Cloud.Firestore;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace TriviaServer.Services
{
    /// Players-only analytics: updates /players/{playerDocId} aggregates.
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
                var projectRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", ".."));
                var fallback = Path.Combine(projectRoot, "secrets", "firebase-analytics.json");
                if (File.Exists(fallback)) credPath = fallback;
            }

            if (string.IsNullOrWhiteSpace(credPath) || !File.Exists(credPath))
                throw new InvalidOperationException("Service-account key not found. Set GOOGLE_APPLICATION_CREDENTIALS or add secrets/firebase-analytics.json.");

            using var doc = JsonDocument.Parse(File.ReadAllText(credPath));
            var projectId = doc.RootElement.TryGetProperty("project_id", out var p) ? p.GetString() : null;
            if (string.IsNullOrWhiteSpace(projectId))
                throw new InvalidOperationException("Service account JSON missing 'project_id'.");

            var credential = GoogleCredential.FromFile(credPath);
            if (credential.IsCreateScopedRequired)
                credential = credential.CreateScoped("https://www.googleapis.com/auth/datastore");

            Console.WriteLine($"[Firestore] Using key: {credPath}");
            Console.WriteLine($"[Firestore] Project: {projectId}");

            _db = new FirestoreDbBuilder { ProjectId = projectId!, Credential = credential }.Build();
        }

        public static FirestoreAnalytics Instance
        {
            get { lock (_lock) return _instance ??= new FirestoreAnalytics(); }
        }

        // Stable, case-insensitive, safe doc id from player name
        private static string PlayerDocId(string name)
        {
            var id = (name ?? "").Trim().ToLowerInvariant();
            id = Regex.Replace(id, @"\s+", " ");
            id = id.Replace("/", "_");
            return string.IsNullOrEmpty(id) ? "_" : id;
        }

        /// Update only player aggregates: name, wins, losses, ties, totalMatches, lastResult, lastMatchAt.
        /// Returns the normalized result string ("win" | "loss" | "tie") that was applied.
        public async Task<string> UpdatePlayerStats(Models.GameOverDto dto)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));
            if (string.IsNullOrWhiteSpace(dto.PlayerName))
                throw new ArgumentException("playerName is required.");
            if (string.IsNullOrWhiteSpace(dto.Result))
                throw new ArgumentException("result is required.");

            var result = dto.Result.Trim().ToLowerInvariant();
            if (result != "win" && result != "loss" && result != "tie")
                throw new ArgumentException("result must be 'win', 'loss', or 'tie'.");

            var playerDocId = PlayerDocId(dto.PlayerName);
            var playerRef = _db.Collection("players").Document(playerDocId);
            var displayName = dto.PlayerName.Trim();

            await _db.RunTransactionAsync(async tx =>
            {
                var snap = await tx.GetSnapshotAsync(playerRef);

                long wins = 0, losses = 0, ties = 0, totalMatches = 0;

                if (snap.Exists)
                {
                    if (snap.ContainsField("wins")) wins = snap.GetValue<long>("wins");
                    if (snap.ContainsField("losses")) losses = snap.GetValue<long>("losses");
                    if (snap.ContainsField("ties")) ties = snap.GetValue<long>("ties");
                    if (snap.ContainsField("totalMatches")) totalMatches = snap.GetValue<long>("totalMatches");
                }

                switch (result)
                {
                    case "win": wins++; break;
                    case "loss": losses++; break;
                    case "tie": ties++; break;
                }
                totalMatches++;

                var updates = new Dictionary<string, object>
                {
                    ["name"] = displayName,
                    ["wins"] = wins,
                    ["losses"] = losses,
                    ["ties"] = ties,
                    ["totalMatches"] = totalMatches,
                    ["lastResult"] = result,
                    ["lastMatchAt"] = Timestamp.FromDateTime(DateTime.UtcNow)
                };

                tx.Set(playerRef, updates, SetOptions.MergeAll);
            });

            return result;
        }
    }
}
