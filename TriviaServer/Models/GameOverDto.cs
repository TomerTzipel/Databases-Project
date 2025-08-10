namespace TriviaServer.Models
{
    public class GameOverDto
    {
        // IDs
        public string MatchId { get; set; } = "";     // optional client GUID
        public string PlayerId { get; set; } = "";    // same id as in your SQL users table
        public string? PlayerName { get; set; }       // optional label

        // Basic stats
        public int Score { get; set; }
        public bool DidWin { get; set; }
        public int NumCorrect { get; set; }
        public int NumQuestions { get; set; }
        public float DurationSeconds { get; set; }    // total match time
    }
}
