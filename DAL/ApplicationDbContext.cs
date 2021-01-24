using System;
using System.Linq;
using Domain;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DAL
{
    public class ApplicationDbContext : DbContext
    {
        public DbSet<Game> Games { get; set; } = null!;
        public DbSet<GameOption> GameOptions { get; set; } = null!;
        public DbSet<Player> Players { get; set; } = null!; 
        public DbSet<GameOptionShip> GameOptionShips { get; set; } = null!;
        public DbSet<GameShip> GameShips { get; set; } = null!;
        public DbSet<Ship> Ships { get; set; } = null!;
        public DbSet<PlayerBoardState> PlayerBoardStates { get; set; } = null!;
        

        private static ILoggerFactory _loggerFactory = LoggerFactory.Create(
            builder =>
            {
                builder
                    .AddFilter("Microsoft", LogLevel.Information).AddConsole();
            }
        );

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        // {
        //     base.OnConfiguring(optionsBuilder);
        //     optionsBuilder
        //         .UseLoggerFactory(_loggerFactory)
        //         .EnableSensitiveDataLogging()
        //         .UseSqlServer(@"Server=(localdb)\mssqllocaldb;Database=BattleShips;Trusted_Connection=True;");
        //     //.UseSqlite(@"Data Source=C:\Users\ottko\RiderProjects\DbDemo\app.db");
        // }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder
                .Entity<Ship>()
                .HasIndex(i => new
                {
                    i.Name,
                    i.Size
                })
                .IsUnique();

            modelBuilder
                .Entity<Game>()
                .HasOne(game => game.PlayerA)
                .WithOne()
                .HasForeignKey<Game>(g => g.PlayerAId);
            
            modelBuilder
                .Entity<Game>()
                .HasOne(game => game.PlayerB)
                .WithOne()
                .HasForeignKey<Game>(g => g.PlayerBId);
            
            // remove the cascade delete
            foreach (var relationship in modelBuilder.Model
                .GetEntityTypes()
                .Where(e => !e.IsOwned())
                .SelectMany(e => e.GetForeignKeys()))
            {
                relationship.DeleteBehavior = DeleteBehavior.Restrict;
            }


        }
        
    }
}