using System;
using System.Linq;
using System.Threading.Tasks;
using Domain;
using GameEngine;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace WebApp.Pages.NextTurn
{
    public class Index : PageModel
    {
        private readonly ILogger<IndexModel> _logger;

        private readonly DAL.ApplicationDbContext _context;
        
        public Index(DAL.ApplicationDbContext context, ILogger<IndexModel> logger)
        {
            _context = context;
            _logger = logger;
        }

        public Game? Game { get; set; }
        public BattleShips? BattleShips { get; set; }
        

        public async Task<IActionResult> OnGetAsync(int id, string? move)
        {
            Game = await _context.Games
                .Where(g => g.GameId == id)
                .Include(g => g.GameOption)
                .Include(g => g.GameOption.GameOptionShips)
                .Include(g => g.PlayerA)
                .Include(g => g.PlayerA.GameShips)
                .Include(g => g.PlayerA.PlayerBoardStates)
                .Include(g => g.PlayerB)
                .Include(g => g.PlayerB.GameShips)
                .Include(g => g.PlayerB.PlayerBoardStates).FirstOrDefaultAsync();
            BattleShips = new BattleShips(Game);
            
            if (move == "go")
            {
                return RedirectToPage("../GamePlay/Index", new { id = Game!.GameId });
            }

            return Page();
        }
    }
}