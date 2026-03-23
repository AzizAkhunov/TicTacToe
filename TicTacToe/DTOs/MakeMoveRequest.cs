namespace TicTacToe.DTOs
{
    public class MakeMoveRequest
    {
        public Guid SessionId { get; set; }
        public string PlayerName { get; set; } = null!;
        public int CellIndex { get; set; }
    }
}
