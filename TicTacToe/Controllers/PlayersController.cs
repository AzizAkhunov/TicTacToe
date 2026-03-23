using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TicTacToe.DTOs;
using TicTacToe.Services;

namespace TicTacToe.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PlayersController : ControllerBase
    {
        private readonly PlayerService _playerService;

        public PlayersController(PlayerService playerService)
        {
            _playerService = playerService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginPlayerRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
                return BadRequest("Player name is required");

            var player = await _playerService.LoginAsync(request.Name);

            return Ok(player);
        }

        [HttpGet("{name}")]
        public async Task<IActionResult> GetByName(string name)
        {
            var player = await _playerService.GetByNameAsync(name);

            if (player is null)
                return NotFound();

            return Ok(player);
        }

        [HttpGet("leaderboard")]
        public async Task<IActionResult> Leaderboard()
        {
            var players = await _playerService.GetLeaderboardAsync();
            return Ok(players);
        }
    }
}
