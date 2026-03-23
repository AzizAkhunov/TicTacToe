using TicTacToe.DTOs;
using TicTacToe.Entities;

namespace TicTacToe.Helpers
{
    public class GameSessionMapper
    {
        public static GameSessionResponseDto ToDto(GameSession session)
        {
            return new GameSessionResponseDto
            {
                Id = session.Id,
                RoomName = session.RoomName,
                Board = session.Board,
                CurrentTurn = session.CurrentTurn,
                Status = session.Status.ToString(),
                Winner = session.Winner,
                PlayerXName = session.PlayerX.Name,
                PlayerOName = session.PlayerO?.Name,
                RematchRequestedByX = session.RematchRequestedByX,
                RematchRequestedByO = session.RematchRequestedByO,
                CreatedAt = session.CreatedAt
            };
        }
    }
}
