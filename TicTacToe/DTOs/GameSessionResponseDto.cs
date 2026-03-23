namespace TicTacToe.DTOs
{
    public class GameSessionResponseDto
    {
        public Guid Id { get; set; }
        public string RoomName { get; set; } = null!;
        public string Board { get; set; } = null!;
        public string CurrentTurn { get; set; } = null!;
        public string Status { get; set; } = null!;
        public string? Winner { get; set; }

        public string PlayerXName { get; set; } = null!;
        public string? PlayerOName { get; set; }

        public bool RematchRequestedByX { get; set; }
        public bool RematchRequestedByO { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
