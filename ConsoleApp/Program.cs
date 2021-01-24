using System;
using System.Threading;
using DAL;
using Domain;
using Domain.Enums;
using GameConsoleUI;
using GameEngine;
using MenuSystem;
using Microsoft.EntityFrameworkCore;


namespace ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            var dbOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlServer(
                    @"Server=(localdb)\mssqllocaldb;
                                Database=BattleShips;
                                Trusted_Connection=True;"
                ).Options;
            var gameOption = new GameOption
            {
                Name = "Default",
                BoardWidth = 10,
                BoardHeight = 10,
                EShipsCanTouch = EShipsCanTouch.No,
                ENextMoveAfterHit = ENextMoveAfterHit.OtherPlayer
                
            };
            var player1 = new Player {Name = "Player 1", EPlayerType = EPlayerType.Human};
            var player2 = new Player {Name = "Player 2", EPlayerType = EPlayerType.Human};

            var game = new Game()
            {
                GameOption = gameOption,
                PlayerA = player1,
                PlayerB = player2
            };
            
            //Ship touching options menu
            var boatTouchingOptionsMenu = new Menu(MenuLevel.Level2Plus);
            boatTouchingOptionsMenu.AddMenuHeader("Ship touching options");
            boatTouchingOptionsMenu.AddMenuItem(new MenuItem("No touching", "1", () =>
            {
                gameOption.EShipsCanTouch = EShipsCanTouch.No;
                return "r";
            }));

            boatTouchingOptionsMenu.AddMenuItem(new MenuItem("Only corners can touch", "2", () =>
            {
                gameOption.EShipsCanTouch = EShipsCanTouch.Corner;
                return "r";
            }));
            boatTouchingOptionsMenu.AddMenuItem(new MenuItem("Boats can touch", "3", () =>
            {
                gameOption.EShipsCanTouch = EShipsCanTouch.Yes;
                return "r";
            }));

            //Next move after hit options menu
            var nextMoveAfterHitMenu = new Menu(MenuLevel.Level2Plus);
            nextMoveAfterHitMenu.AddMenuHeader("Next move after hit options");
            nextMoveAfterHitMenu.AddMenuItem(new MenuItem("Same Player", "1", () =>
            {
                gameOption.ENextMoveAfterHit = ENextMoveAfterHit.SamePlayer;
                return "r";
            }));
            nextMoveAfterHitMenu.AddMenuItem(new MenuItem("Other Player", "2", () =>
            {
                gameOption.ENextMoveAfterHit = ENextMoveAfterHit.OtherPlayer;
                return "r";
            }));

            var shipOptionMenu = new Menu(MenuLevel.Level2Plus);
            shipOptionMenu.AddMenuHeader("Ship options");

            //Game Options Menu
            var gameOptionsMenu = new Menu(MenuLevel.Level2Plus);
            gameOptionsMenu.AddMenuHeader("Game Options");
            gameOptionsMenu.AddMenuItem(new MenuItem("All Defaults", "D", () =>
            {
                gameOption.BoardHeight = 10;
                gameOption.BoardWidth = 10;
                gameOption.ENextMoveAfterHit = ENextMoveAfterHit.OtherPlayer;
                gameOption.EShipsCanTouch = EShipsCanTouch.No;

                return "p";
            }));
            gameOptionsMenu.AddMenuItem(new MenuItem("Set Board size", "1", () =>
                {
                    var width = UserInputValidation("width", 10, 20);
                    var height = UserInputValidation("height", 10, 20);
                    gameOption.BoardHeight = height;
                    gameOption.BoardWidth = width;
                    return "";
                }
            ));
            gameOptionsMenu.AddMenuItem(new MenuItem("Use default Board 10x10", "2", () =>
                {
                    gameOption.BoardHeight = 10;
                    gameOption.BoardWidth = 10;
                    return "";
                }
            ));
            gameOptionsMenu.AddMenuItem(new MenuItem("Ship touching options", "3", boatTouchingOptionsMenu.RunMenu));
            gameOptionsMenu.AddMenuItem(new MenuItem("Next move after hit", "4", nextMoveAfterHitMenu.RunMenu));
            
            var newGameMenu = new Menu(MenuLevel.Level1);
            newGameMenu.AddMenuHeader("<=I=I=> BATTLESHIPS <=I=I=>");
            newGameMenu.AddMenuItem(new MenuItem("Start Game", "S", () =>
            {
                //game = new Game {GameOption = gameOption, PlayerA = player1, PlayerB = player2};
                player1.Name = BattleShipsConsoleUi.GetPlayerName("Player 1");
                
                player2.Name = BattleShipsConsoleUi.GetPlayerName("Player 2");

                var battleShips = new BattleShips(game);
                PlayBattleShips(battleShips, dbOptions);
                
                
                return "m";
            }));
            newGameMenu.AddMenuItem(new MenuItem("Game Options", "O", gameOptionsMenu.RunMenu));
            
            //Main Menu
            var menu = new Menu(MenuLevel.Level0);
            menu.AddMenuHeader("<=I=I=> BATTLESHIPS <=I=I=>");
            menu.AddMenuItem(new MenuItem("New Game Human vs Human", "1", newGameMenu.RunMenu));
            
            menu.AddMenuItem(new MenuItem(
                "Load Game",
                "l",
                () =>
                {
                    game = new Game {GameOption = gameOption, PlayerA = player1, PlayerB = player2};
                    var battleShips = new BattleShips(game);
                    LoadGameAction(battleShips, dbOptions);
                    return "";
                })
            );
            menu.RunMenu();
        }
        

        private static bool AddShips(BattleShips battleShips, DbContextOptions<ApplicationDbContext> dbOptions)
        {
            
            var wantsToQuit = false;
            int unAddedShipCount;
            do
            {
                Console.Clear();
                var player = battleShips.GetPlayer(true);
                Console.WriteLine($"{player.Name} choose a ship to add:");
                var gameShips = battleShips.GetUnaddedGameShips();

                var gameShip = BattleShipsConsoleUi.DrawShipList(gameShips, player.Name);

                var (x, y) = GetCoordinates(
                    battleShips,
                    gameShip.Size,
                    true);

                if (x == 100 && y == 100)
                {
                    SaveGameMenu(battleShips, dbOptions);
                    wantsToQuit = true;
                    break;
                }

                var isHorizontal = BattleShipsConsoleUi.IsShipHorizontal;
                battleShips.AddShips(gameShip, x, y, isHorizontal);
                
                unAddedShipCount = battleShips.GetUnaddedGameShips().Count;
            } while (unAddedShipCount > 0);

            return wantsToQuit;
        }

        private static string PlayBattleShips(BattleShips battleShips,DbContextOptions<ApplicationDbContext> dbOptions)
        {
            
            if (battleShips.GetUnaddedGameShips().Count > 0)
            {
                var userWantsToQuit = AddShips(battleShips, dbOptions);
                if (userWantsToQuit)
                {
                    return "";
                }
                Console.Clear();
            }else if (battleShips.IsGameOver())
            {
                BattleShipsConsoleUi.GameOverUi(battleShips);
                return "";
            }

            do
            {
                Console.WriteLine("Press ESC to return back to menu");
                var (x, y) = GetCoordinates(battleShips);
                // Exit Game if x == 100 & y == 100
                if (x == 100 && y == 100)
                {
                    SaveGameMenu(battleShips, dbOptions);
                    break;
                }

                var (successfulMove, playerBoardState) = battleShips.MakeMove(x, y);
                //isGameOver
                if (successfulMove[1])
                {
                    BattleShipsConsoleUi.GameOverUi(battleShips);
                    SaveGameMenu(battleShips, dbOptions);
                    break;
                }

            } while (true);
            
            return "";
        }


        private static (int x, int y) GetCoordinates(
            BattleShips battleShips,
            int size = 0,
            bool displayOneBoard = false)
        {
            BattleShipsConsoleUi.WriteBoardStartInstruction();
            BattleShipsConsoleUi.WriteMinMaxInstruction(battleShips.GetBoards()[0], displayOneBoard);
            var (x, y) = BattleShipsConsoleUi.BoardControl(battleShips, size, displayOneBoard);

            return (x, y);
        }


        private static void SaveGameMenu(BattleShips battleShips,DbContextOptions<ApplicationDbContext> dbOptions)
        {
            var menuSaveGame = new Menu(MenuLevel.LevelBlank);
            menuSaveGame.AddMenuHeader("Do you want to save game?");
            menuSaveGame.AddMenuItem(new MenuItem("Yes", "Y", () =>
            {
                battleShips.SaveGame(dbOptions);
                
                return "m";
            }));
            menuSaveGame.AddMenuItem(new MenuItem("No", "N", () => "m"));
            menuSaveGame.RunMenu();
        }

        private static string LoadGameAction(BattleShips battleShips, DbContextOptions<ApplicationDbContext> dbOptions)
        {
            Console.Clear();
           
            var games = BattleShips.LoadSavedGames(dbOptions);
            
            var menuGames = new Menu(MenuLevel.Level2Plus);

            menuGames.AddMenuHeader("Saved Games");
            foreach (var game in games)
            {
                menuGames.AddMenuItem(new MenuItem($"{game.PlayerA.Name} vs {game.PlayerB.Name} - {game.Description}", $"{game.GameId}", () =>
                    {
                        battleShips.LoadGame(game.GameId, dbOptions);
                        PlayBattleShips(battleShips, dbOptions);

                        return "";
                    }
                ));
            }
            menuGames.RunMenu();
            return "m";
        }
        private static int UserInputValidation(string valueNeeded, int rangeStart, int rangeEnd, bool isBoardSide = true)
        {
            int value;
            do
            {
                Console.Write(isBoardSide ? $"Enter Board {valueNeeded} (min 10, max 20): " : $"Amount of this ship: ");

                var userInput = Console.ReadLine()?.Trim();
                var isNumerical = int.TryParse(userInput, out var result);
                Console.WriteLine(userInput);
                if (isNumerical && result >= rangeStart && result <= rangeEnd)
                {
                    value = result;
                    break;
                }

                Console.WriteLine("There was a problem with your input, please check values!");
            } while (true);

            return value;
        }
        
    }
}