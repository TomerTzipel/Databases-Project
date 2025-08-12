namespace TriviaServer.Models
{
    public class GameOverDto
    {
        // Player identity for analytics (no ID)
        public string PlayerName { get; set; } = string.Empty;

        // Required: "win" | "loss" | "tie"
        public string Result { get; set; } = string.Empty;
    }
}
