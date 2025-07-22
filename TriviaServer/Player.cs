namespace TriviaServer
{
    public class Player
    {
        public string? Name { get; set; }
        public int Score { get; set; } = 0;
        public float TotalTime { get; set; } = 0;
        public bool IsActive { get; set; }
    }

}
