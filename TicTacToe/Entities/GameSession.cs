using TicTacToe.Enums;

namespace TicTacToe.Entities
{
    public class GameSession
    {
        public Guid Id { get; set; }
        public string RoomName { get; set; } = null!;

        public Guid PlayerXId { get; set; }
        public Player PlayerX { get; set; } = null!;

        public Guid? PlayerOId { get; set; }
        public Player? PlayerO { get; set; }

        public string Board { get; set; } = "---------";
        public string CurrentTurn { get; set; } = "X";
        public GameStatus Status { get; set; } = GameStatus.Waiting;
        public string? Winner { get; set; }

        public bool RematchRequestedByX { get; set; }
        public bool RematchRequestedByO { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
