using Microsoft.EntityFrameworkCore;
using TicTacToe.Data;
using TicTacToe.Entities;

namespace TicTacToe.Services
{
    public class PlayerService
    {
        private readonly AppDbContext _context;

        public PlayerService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Player> LoginAsync(string name)
        {
            name = name.Trim();

            var existingPlayer = await _context.Players
                .FirstOrDefaultAsync(p => p.Name.ToLower() == name.ToLower());

            if (existingPlayer is not null)
                return existingPlayer;

            var player = new Player
            {
                Name = name
            };

            _context.Players.Add(player);
            await _context.SaveChangesAsync();

            return player;
        }

        public async Task<Player?> GetByNameAsync(string name)
        {
            return await _context.Players
                .FirstOrDefaultAsync(p => p.Name.ToLower() == name.ToLower());
        }

        public async Task<List<Player>> GetLeaderboardAsync()
        {
            return await _context.Players
                .OrderByDescending(p => p.Wins)
                .ThenByDescending(p => p.GamesPlayed)
                .Take(20)
                .ToListAsync();
        }
    }
}
