namespace TriviaServer.Models
{
    public class GameOverDto
    {
        public string PlayerName { get; set; } = "";

        public int NumCorrect { get; set; }
        public float DurationSeconds { get; set; }
        public int Outcome { get; set; } // -1 = none ,0 = win, 1 = lose, 2 = tie
    }

    public class PlayerAnalytics
    {
        public long TotalGamesPlayed { get; set; }
        public long TotalWins { get; set; }
        public long TotalLosses { get; set; }
        public long TotalTies { get; set; }
        public double AverageDurationSeconds { get; set; }
    }
}
