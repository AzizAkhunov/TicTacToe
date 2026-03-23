using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using TicTacToe.DTOs;
using TicTacToe.Hubs;
using TicTacToe.Services;

namespace TicTacToe.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SessionsController : ControllerBase
{
    private readonly GameSessionService _sessionService;
    private readonly PlayerService _playerService;
    private readonly IHubContext<GameHub> _hubContext;

    public SessionsController(
        GameSessionService sessionService,
        PlayerService playerService,
        IHubContext<GameHub> hubContext)
    {
        _sessionService = sessionService;
        _playerService = playerService;
        _hubContext = hubContext;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var sessions = await _sessionService.GetOpenSessionsAsync();
        return Ok(sessions);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var session = await _sessionService.GetByIdAsync(id);

        if (session is null)
            return NotFound();

        return Ok(session);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateSessionRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.RoomName))
            return BadRequest("Room name is required");

        if (string.IsNullOrWhiteSpace(request.PlayerName))
            return BadRequest("Player name is required");

        var player = await _playerService.GetByNameAsync(request.PlayerName);

        if (player is null)
            return BadRequest("Player not found");

        var session = await _sessionService.CreateSessionAsync(request.RoomName, player);

        var sessions = await _sessionService.GetOpenSessionsAsync();
        await _hubContext.Clients.Group("lobby").SendAsync("LobbyUpdated", sessions);

        return Ok(session);
    }

    [HttpPost("{id:guid}/join")]
    public async Task<IActionResult> Join(Guid id, [FromBody] JoinSessionRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.PlayerName))
            return BadRequest("Player name is required");

        var player = await _playerService.GetByNameAsync(request.PlayerName);

        if (player is null)
            return BadRequest("Player not found");

        var result = await _sessionService.JoinSessionAsync(id, player);

        if (!result.Success)
            return BadRequest(result.Message);

        var sessions = await _sessionService.GetOpenSessionsAsync();
        await _hubContext.Clients.Group("lobby").SendAsync("LobbyUpdated", sessions);
        await _hubContext.Clients.Group(id.ToString()).SendAsync("GameUpdated", result.Session);

        return Ok(result.Session);
    }

    [HttpPost("{id:guid}/leave")]
    public async Task<IActionResult> Leave(Guid id, [FromBody] LeaveSessionRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request.PlayerName))
            return BadRequest("Player name is required");

        var result = await _sessionService.LeaveSessionAsync(id, request.PlayerName);

        if (!result.Success)
            return BadRequest(result.Message);

        var sessions = await _sessionService.GetOpenSessionsAsync();
        await _hubContext.Clients.Group("lobby").SendAsync("LobbyUpdated", sessions);
        await _hubContext.Clients.Group(id.ToString()).SendAsync("SessionLeft", new
        {
            result.Message,
            result.Session
        });

        return Ok(new
        {
            result.Message,
            result.Session
        });
    }
}