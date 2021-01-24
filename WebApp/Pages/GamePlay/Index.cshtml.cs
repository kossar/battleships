using System;
using System.Linq;
using System.Threading.Tasks;
using Domain;
using GameEngine;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace WebApp.Pages.GamePlay
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

        [BindProperty(SupportsGet = true)] public int PosX { get; set; } = 0;

        [BindProperty(SupportsGet = true)] public int PosY { get; set; } = 0;

        public GameShip? CurrentGameShip { get; set; }
        
        public BoardSquareState[,] CurrentPlayerBoard { get; set; } = default!;
        public BoardSquareState[,]? OtherPlayerBoard { get; set; }

        public bool IsGameOver { get; set; }
        public bool IsShipHorizontal { get; set; }

        private bool _changePlayer; // To display other screen before other player makes move

        public async Task<IActionResult> OnGetAsync(int id, int? shipId, int? x, int? y, string dir, bool horizontal, string move)
        {
            IsShipHorizontal = horizontal;


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

            
            if (BattleShips.GetUnaddedGameShips().Count > 0)
            {
                CurrentGameShip = shipId != null ? BattleShips.GetUnaddedGameShips().FirstOrDefault(s => s.GameShipId == shipId) : BattleShips.GetUnaddedGameShips().First();
            }

            IsGameOver = BattleShips.IsGameOver();
            
            if (x != null && y != null && CurrentGameShip != null)
            {
                var (isSuccess, playerBoardState) = BattleShips.AddShips(CurrentGameShip, x.Value, y.Value, IsShipHorizontal);
                if (isSuccess && playerBoardState != null)
                {
                    _context.Add(playerBoardState);
                    if (BattleShips.GetUnaddedGameShips().Count > 0)
                    {
                        CurrentGameShip = BattleShips.GetUnaddedGameShips().First();
                    }
                    else
                    {
                        CurrentGameShip = null;
                        _changePlayer = true;
                    }
                    
                }
            }else if (x != null && y != null && move == "shoot")
            {
                var (success, playerBoardState) = BattleShips.MakeMove(x.Value, y.Value);
                
                if (success[0] && !success[1] && playerBoardState != null)
                {
                    _context.Add(playerBoardState);
                    _changePlayer = true;
                }else if (success[1] && playerBoardState != null)
                {
                    _context.Add(playerBoardState);
                    IsGameOver = true;
                }

            }

            CurrentPlayerBoard = BattleShips.GetBoards().First();
            if (BattleShips.GetUnaddedGameShips().Count == 0 && !IsGameOver)
            {
                OtherPlayerBoard = BattleShips.GetBoards(true)[1];
            }else if (IsGameOver)
            {
                OtherPlayerBoard = BattleShips.GetBoards(false, true)[1];
            }

            if (CurrentGameShip != null)
            {
                HandlePosition(dir);
            }
            
            
            await _context.SaveChangesAsync();

            if (_changePlayer)
            {
                return RedirectToPage("../NextTurn/Index", new { id = Game!.GameId });
            }
            return Page();    
            
        }

       

        private void HandlePosition(string? dir)
        {
            switch (dir)
            {
                case "up-left":
                    PosY--;
                    PosX--;
                    break;
                case "up":
                    PosY--;
                    break;
                case "up-right":
                    PosY--;
                    PosX++;
                    break;
                case "left":
                    PosX--;
                    break;
                case "right":
                    PosX++;
                    break;
                case "down-left":
                    PosY++;
                    PosX--;
                    break;
                case "down":
                    PosY++;
                    break;
                case "down-right":
                    PosY++;
                    PosX++;
                    break;
                case "rotate":
                    IsShipHorizontal = !IsShipHorizontal;
                    break;
            }
            
            if (IsShipHorizontal && PosX < 0)
            {
                PosX = Game!.GameOption.BoardWidth - CurrentGameShip!.Size;
            } 
            if (!IsShipHorizontal && PosX < 0)
            {
                PosX = Game!.GameOption.BoardWidth - 1;
            }

            if (!IsShipHorizontal && PosY < 0)
            {
                PosY = Game!.GameOption.BoardHeight - CurrentGameShip!.Size;
                
                
            }
            if (IsShipHorizontal && PosY < 0)
            {
                PosY = Game!.GameOption.BoardHeight - 1;
            }

            if (!IsShipHorizontal && PosX > Game!.GameOption.BoardWidth - 1 || IsShipHorizontal && PosX + CurrentGameShip!.Size > Game!.GameOption.BoardWidth)
            {
                PosX = 0;
                
            }

            if (IsShipHorizontal && PosY  > Game!.GameOption.BoardHeight - 1 || !IsShipHorizontal && PosY + CurrentGameShip!.Size > Game!.GameOption.BoardHeight)
            {
                PosY = 0;
                
            }
        }
    }
}