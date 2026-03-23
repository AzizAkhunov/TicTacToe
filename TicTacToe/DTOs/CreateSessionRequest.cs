namespace TicTacToe.DTOs
{
    public class CreateSessionRequest
    {
        public string RoomName { get; set; } = null!;
        public string PlayerName { get; set; } = null!;
    }
}
