using Microsoft.EntityFrameworkCore;
using TicTacToe.Entities;

namespace TicTacToe.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Player> Players => Set<Player>();
        public DbSet<GameSession> GameSessions => Set<GameSession>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Player>()
                .HasIndex(p => p.Name)
                .IsUnique();

            modelBuilder.Entity<Player>()
                .Property(p => p.Name)
                .HasMaxLength(50);

            modelBuilder.Entity<GameSession>()
                .Property(g => g.RoomName)
                .HasMaxLength(100);

            modelBuilder.Entity<GameSession>()
                .Property(g => g.Board)
                .HasMaxLength(9);

            modelBuilder.Entity<GameSession>()
                .HasOne(g => g.PlayerX)
                .WithMany()
                .HasForeignKey(g => g.PlayerXId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<GameSession>()
                .HasOne(g => g.PlayerO)
                .WithMany()
                .HasForeignKey(g => g.PlayerOId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
