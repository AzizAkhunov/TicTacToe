namespace TicTacToe.Entities
{
    public class Player
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;

        public int Wins { get; set; }
        public int Losses { get; set; }
        public int Draws { get; set; }
        public int GamesPlayed { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
