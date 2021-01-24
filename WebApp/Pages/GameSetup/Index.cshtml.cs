using System;
using System.Linq;
using System.Threading.Tasks;
using Domain;
using GameEngine;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebApp.Pages.GameSetup
{
    public class Index : PageModel
    {
        private readonly DAL.ApplicationDbContext _context;

        public Index(DAL.ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult OnGet()
        {
            return Page();
        }

        [BindProperty] 
        public Player? PlayerA { get; set; } = default!;

        [BindProperty]
        public Player? PlayerB { get; set; } = default!;
        
        [BindProperty]
        public GameOption? GameOption { get; set; }


        // To protect from overposting attacks, see https://aka.ms/RazorPagesCRUD
        public async Task<IActionResult> OnPostAsync()
        {
            // Console.WriteLine(GameOption!.EShipsCanTouch);
            // if (!ModelState.IsValid)
            // {
            //     Console.WriteLine("Not Valid");
            //     return Page();
            // }

            var game = new Game()
            {
                GameOption = GameOption!,
                PlayerA = PlayerA!,
                PlayerB = PlayerB!

            };
            BattleShips battleShips = new(game);
            var ships = battleShips.GetAvailableShips();

            
            foreach (var ship in ships.Where(ship => !_context.Ships.Any(s => s.Size == ship.Size && s.Name == ship.Name)))
            {
                _context.Ships.Add(ship);
            }

            await _context.SaveChangesAsync();
            
            _context.Games.Add(game);
            //_context.GameOptions.Add(game.GameOption);
            foreach (var gameOptionShip in battleShips.GetGameOptionShips())
            {
                var dbShip = _context.Ships.First(s => s.Size == gameOptionShip.Ship.Size && s.Name == gameOptionShip.Ship.Name);
                gameOptionShip.Ship = dbShip;
                _context.GameOptionShips.Add(gameOptionShip);
            }

            foreach (var gameShip in battleShips.GetGameShips(true))
            {
                _context.GameShips.Add(gameShip);
            }

            foreach (var gameShip in battleShips.GetGameShips(false))
            {
                _context.GameShips.Add(gameShip);
            }
            await _context.SaveChangesAsync();

            return RedirectToPage("../GamePlay/Index", new { id = game.GameId });
        }
    }
}