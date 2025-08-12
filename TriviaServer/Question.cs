namespace TriviaServer
{
    public class Question
    {
        public string? QuestionText { get; set; }
        public string[] OptionTexts { get; set; } = new string[4];
        public int AnswerIndex { get; set; }
    }

    public class SearchResult
    {
        public bool WasFound { get; set; }
        public string? PlayerName { get; set; }
    }

    public class QuestionResult
    {
        public int Time { get; set; }
        public bool Result { get; set; }
    }

    public class GameResult
    {
        public int TotalTime { get; set; }
        public int Score { get; set; }
    }
}
