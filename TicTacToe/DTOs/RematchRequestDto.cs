namespace TicTacToe.DTOs
{
    public class RematchRequestDto
    {
        public Guid SessionId { get; set; }
        public string PlayerName { get; set; } = null!;
    }
}
