namespace TriviaServer.Models
{
    public class GameOverDto
    {
        public string MatchId { get; set; } = "";
        public string PlayerId { get; set; } = "";
        public int Score { get; set; }
        public float DurationSeconds { get; set; }
        public List<string>? QuestionIds { get; set; }
        public List<int>? Answers { get; set; }
        public List<int>? CorrectIndexes { get; set; }
        public string? Mode { get; set; }
        public string? Build { get; set; }
    }
}
