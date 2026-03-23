using Microsoft.EntityFrameworkCore;
using TicTacToe.Data;
using TicTacToe.DTOs;
using TicTacToe.Entities;
using TicTacToe.Enums;
using TicTacToe.Helpers;

namespace TicTacToe.Services;

public class GameSessionService
{
    private readonly AppDbContext _context;

    public GameSessionService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<GameSessionResponseDto>> GetOpenSessionsAsync()
    {
        var sessions = await _context.GameSessions
            .Include(g => g.PlayerX)
            .Include(g => g.PlayerO)
            .OrderByDescending(g => g.CreatedAt)
            .ToListAsync();

        return sessions.Select(GameSessionMapper.ToDto).ToList();
    }

    public async Task<GameSession?> GetEntityByIdAsync(Guid id)
    {
        return await _context.GameSessions
            .Include(g => g.PlayerX)
            .Include(g => g.PlayerO)
            .FirstOrDefaultAsync(g => g.Id == id);
    }

    public async Task<GameSessionResponseDto?> GetByIdAsync(Guid id)
    {
        var session = await GetEntityByIdAsync(id);
        return session is null ? null : GameSessionMapper.ToDto(session);
    }

    public async Task<GameSessionResponseDto> CreateSessionAsync(string roomName, Player player)
    {
        var session = new GameSession
        {
            RoomName = roomName.Trim(),
            PlayerXId = player.Id,
            Status = GameStatus.Waiting,
            Board = "---------",
            CurrentTurn = "X",
            Winner = null,
            RematchRequestedByX = false,
            RematchRequestedByO = false
        };

        _context.GameSessions.Add(session);
        await _context.SaveChangesAsync();

        var created = await GetEntityByIdAsync(session.Id) ?? session;
        return GameSessionMapper.ToDto(created);
    }

    public async Task<(bool Success, string Message, GameSessionResponseDto? Session)> JoinSessionAsync(Guid sessionId, Player player)
    {
        var session = await _context.GameSessions
            .Include(g => g.PlayerX)
            .Include(g => g.PlayerO)
            .FirstOrDefaultAsync(g => g.Id == sessionId);

        if (session is null)
            return (false, "Session not found", null);

        if (session.PlayerXId == player.Id)
            return (false, "You cannot join your own room as second player", null);

        if (session.PlayerOId.HasValue)
            return (false, "Session is already full", null);

        if (session.Status != GameStatus.Waiting)
            return (false, "Session is not available", null);

        session.PlayerOId = player.Id;
        session.Status = GameStatus.InProgress;

        await _context.SaveChangesAsync();

        var updated = await GetEntityByIdAsync(session.Id);

        return updated is null
            ? (false, "Session load failed", null)
            : (true, "Joined successfully", GameSessionMapper.ToDto(updated));
    }

    public async Task<MoveResultDto> MakeMoveAsync(Guid sessionId, string playerName, int cellIndex)
    {
        var session = await _context.GameSessions
            .Include(g => g.PlayerX)
            .Include(g => g.PlayerO)
            .FirstOrDefaultAsync(g => g.Id == sessionId);

        if (session is null)
        {
            return new MoveResultDto
            {
                Success = false,
                Message = "Session not found"
            };
        }

        if (session.Status != GameStatus.InProgress)
        {
            return new MoveResultDto
            {
                Success = false,
                Message = "Game is not in progress"
            };
        }

        if (cellIndex < 0 || cellIndex > 8)
        {
            return new MoveResultDto
            {
                Success = false,
                Message = "Invalid cell index"
            };
        }

        var normalizedPlayerName = playerName.Trim().ToLower();
        string? symbol = null;

        if (session.PlayerX.Name.ToLower() == normalizedPlayerName)
            symbol = "X";
        else if (session.PlayerO is not null && session.PlayerO.Name.ToLower() == normalizedPlayerName)
            symbol = "O";

        if (symbol is null)
        {
            return new MoveResultDto
            {
                Success = false,
                Message = "Player is not part of this session"
            };
        }

        if (session.CurrentTurn != symbol)
        {
            return new MoveResultDto
            {
                Success = false,
                Message = "It is not your turn"
            };
        }

        var boardArray = session.Board.ToCharArray();

        if (boardArray[cellIndex] != '-')
        {
            return new MoveResultDto
            {
                Success = false,
                Message = "Cell is already occupied"
            };
        }

        boardArray[cellIndex] = symbol[0];
        session.Board = new string(boardArray);

        var result = GameLogicHelper.CheckWinner(session.Board);

        if (result == "X" || result == "O")
        {
            session.Status = GameStatus.Finished;
            session.Winner = result;
            await UpdatePlayerStatsAsync(session, result);
        }
        else if (result == "Draw")
        {
            session.Status = GameStatus.Finished;
            session.Winner = "Draw";
            await UpdatePlayerStatsAsync(session, "Draw");
        }
        else
        {
            session.CurrentTurn = symbol == "X" ? "O" : "X";
        }

        await _context.SaveChangesAsync();

        var updated = await GetEntityByIdAsync(session.Id);

        return new MoveResultDto
        {
            Success = true,
            Message = "Move successful",
            Session = updated is null ? null : GameSessionMapper.ToDto(updated)
        };
    }

    public async Task<(bool Success, string Message, GameSessionResponseDto? Session)> LeaveSessionAsync(Guid sessionId, string playerName)
    {
        var session = await _context.GameSessions
            .Include(g => g.PlayerX)
            .Include(g => g.PlayerO)
            .FirstOrDefaultAsync(g => g.Id == sessionId);

        if (session is null)
            return (false, "Session not found", null);

        var normalizedPlayerName = playerName.Trim().ToLower();

        var isPlayerX = session.PlayerX.Name.ToLower() == normalizedPlayerName;
        var isPlayerO = session.PlayerO is not null && session.PlayerO.Name.ToLower() == normalizedPlayerName;

        if (!isPlayerX && !isPlayerO)
            return (false, "Player is not part of this session", null);

        if (isPlayerO)
        {
            session.PlayerOId = null;
            session.PlayerO = null;
            session.Status = GameStatus.Waiting;
            session.Board = "---------";
            session.CurrentTurn = "X";
            session.Winner = null;
            session.RematchRequestedByX = false;
            session.RematchRequestedByO = false;

            await _context.SaveChangesAsync();

            var updated = await GetEntityByIdAsync(session.Id);
            return updated is null
                ? (false, "Session load failed", null)
                : (true, "Player O left the session", GameSessionMapper.ToDto(updated));
        }

        if (isPlayerX)
        {
            _context.GameSessions.Remove(session);
            await _context.SaveChangesAsync();

            return (true, "Session deleted because host left", null);
        }

        return (false, "Unexpected leave error", null);
    }

    public async Task<(bool Success, string Message, GameSessionResponseDto? Session)> RequestRematchAsync(Guid sessionId, string playerName)
    {
        var session = await _context.GameSessions
            .Include(g => g.PlayerX)
            .Include(g => g.PlayerO)
            .FirstOrDefaultAsync(g => g.Id == sessionId);

        if (session is null)
            return (false, "Session not found", null);

        if (session.Status != GameStatus.Finished)
            return (false, "Rematch is available only after the game is finished", null);

        var normalizedPlayerName = playerName.Trim().ToLower();

        var isPlayerX = session.PlayerX.Name.ToLower() == normalizedPlayerName;
        var isPlayerO = session.PlayerO is not null && session.PlayerO.Name.ToLower() == normalizedPlayerName;

        if (!isPlayerX && !isPlayerO)
            return (false, "Player is not part of this session", null);

        if (isPlayerX)
            session.RematchRequestedByX = true;

        if (isPlayerO)
            session.RematchRequestedByO = true;

        if (session.PlayerO is not null && session.RematchRequestedByX && session.RematchRequestedByO)
        {
            session.Board = "---------";
            session.CurrentTurn = "X";
            session.Status = GameStatus.InProgress;
            session.Winner = null;
            session.RematchRequestedByX = false;
            session.RematchRequestedByO = false;
        }

        await _context.SaveChangesAsync();

        var updated = await GetEntityByIdAsync(session.Id);

        return updated is null
            ? (false, "Session load failed", null)
            : (true, "Rematch state updated", GameSessionMapper.ToDto(updated));
    }

    private async Task UpdatePlayerStatsAsync(GameSession session, string result)
    {
        var playerX = await _context.Players.FirstAsync(p => p.Id == session.PlayerXId);
        var playerO = await _context.Players.FirstAsync(p => p.Id == session.PlayerOId);

        playerX.GamesPlayed++;
        playerO.GamesPlayed++;

        if (result == "X")
        {
            playerX.Wins++;
            playerO.Losses++;
        }
        else if (result == "O")
        {
            playerO.Wins++;
            playerX.Losses++;
        }
        else
        {
            playerX.Draws++;
            playerO.Draws++;
        }
    }
}