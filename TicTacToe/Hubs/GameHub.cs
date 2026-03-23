using Microsoft.AspNetCore.SignalR;
using TicTacToe.Services;

namespace TicTacToe.Hubs;

public class GameHub : Hub
{
    private readonly GameSessionService _gameSessionService;

    public GameHub(GameSessionService gameSessionService)
    {
        _gameSessionService = gameSessionService;
    }

    public async Task JoinLobby()
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, "lobby");
    }

    public async Task LeaveLobby()
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, "lobby");
    }

    public async Task JoinRoom(string sessionId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, sessionId);
    }

    public async Task LeaveRoom(string sessionId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, sessionId);
    }

    public async Task MakeMove(string sessionId, string playerName, int cellIndex)
    {
        if (!Guid.TryParse(sessionId, out var parsedSessionId))
        {
            await Clients.Caller.SendAsync("MoveRejected", "Invalid session id");
            return;
        }

        var result = await _gameSessionService.MakeMoveAsync(parsedSessionId, playerName, cellIndex);

        if (!result.Success)
        {
            await Clients.Caller.SendAsync("MoveRejected", result.Message);
            return;
        }

        await Clients.Group(sessionId).SendAsync("GameUpdated", result.Session);

        if (result.Session is not null && result.Session.Status == "Finished")
        {
            await Clients.Group(sessionId).SendAsync("GameFinished", result.Session);
        }

        await BroadcastLobbyUpdate();
    }

    public async Task RequestRematch(string sessionId, string playerName)
    {
        if (!Guid.TryParse(sessionId, out var parsedSessionId))
        {
            await Clients.Caller.SendAsync("RematchRejected", "Invalid session id");
            return;
        }

        var result = await _gameSessionService.RequestRematchAsync(parsedSessionId, playerName);

        if (!result.Success)
        {
            await Clients.Caller.SendAsync("RematchRejected", result.Message);
            return;
        }

        await Clients.Group(sessionId).SendAsync("RematchUpdated", result.Session);
        await Clients.Group(sessionId).SendAsync("GameUpdated", result.Session);

        await BroadcastLobbyUpdate();
    }

    public async Task PlayerLeaveSession(string sessionId, string playerName)
    {
        if (!Guid.TryParse(sessionId, out var parsedSessionId))
        {
            await Clients.Caller.SendAsync("LeaveRejected", "Invalid session id");
            return;
        }

        var result = await _gameSessionService.LeaveSessionAsync(parsedSessionId, playerName);

        if (!result.Success)
        {
            await Clients.Caller.SendAsync("LeaveRejected", result.Message);
            return;
        }

        await Clients.Group(sessionId).SendAsync("SessionLeft", new
        {
            result.Message,
            result.Session
        });

        await BroadcastLobbyUpdate();
    }

    private async Task BroadcastLobbyUpdate()
    {
        var sessions = await _gameSessionService.GetOpenSessionsAsync();
        await Clients.Group("lobby").SendAsync("LobbyUpdated", sessions);
    }
}