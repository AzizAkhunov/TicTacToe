using TicTacToe.Entities;

namespace TicTacToe.DTOs
{
    public class MoveResultDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = null!;
        public GameSessionResponseDto? Session { get; set; }
    }
}
