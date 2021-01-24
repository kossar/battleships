using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using DAL;
using Domain;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace GameEngine
{
    public class BattleShips
    {
        private Game _game;

        private List<GameOptionShip> _gameOptionShips = new();
        private List<Ship> _ships = new();

        private int _width;
        private int _height;

        private Player _playerA;
        private List<GameShip> PlayerAGameShips { get; set; } = new();

        private List<GameShip> PlayerAUnAddedGameShips { get; set; } = new();

        private BoardSquareState[,] _boardPlayerA;
        private List<PlayerBoardState> _playerABoardStates = new ();

        private Player _playerB;
        private List<GameShip> PlayerBGameShips { get; set; } = new();

        private List<GameShip> PlayerBUnAddedGameShips { get; set; } = new();

        private BoardSquareState[,] _boardPlayerB;
        private List<PlayerBoardState> _playerBBoardStates = new ();
        public int MoveCounter { get; private set; } = 1;
        private bool NextMoveByA { get; set; } = true;

        private readonly Validator _validator;

        public BattleShips(Game game)
        {
            _game = game;
            _playerA = game.PlayerA;
            _playerB = game.PlayerB;

            _width = game.GameOption.BoardWidth;
            _height = game.GameOption.BoardHeight;
            _boardPlayerA = InitEmptyBoard();
            _boardPlayerB = InitEmptyBoard();
            SetGame();
            _validator = new Validator(_game.GameOption.EShipsCanTouch);
        }


        private void SetGame()
        {
            
            if (_game.GameId != 0)
            {
                SetGameFromDb(_game);
               // Console.WriteLine("Game id != 0");
            }
            else
            {
               // Console.WriteLine("Game id is 0");
                var ships = GetAvailableShips();

                var counter = 1;
                foreach (var ship in ships)
                {
                    _ships.Add(ship);
                    GameOptionShip gameOptionShip = new()
                    {
                        Ship = ship, GameOption = _game.GameOption, Amount = 1
                    };
                    for (var i = 0; i < gameOptionShip.Amount; i++)
                    {
                        var gameShipPlayerA = new GameShip()
                        {
                            GameShipOnBoard = counter + i,
                            Size = ship.Size,
                            Name = _game.PlayerA.Name + "_" + ship.Name,
                            IsSunken = false,
                            Player = _game.PlayerA
                        };

                        PlayerAGameShips.Add(gameShipPlayerA);

                        var gameShipPlayerB = new GameShip()
                        {
                            GameShipOnBoard = counter + i,
                            Size = ship.Size,
                            Name = _game.PlayerB.Name + "_" + ship.Name,
                            IsSunken = false,
                            Player = _game.PlayerB
                        };
                        PlayerBGameShips.Add(gameShipPlayerB);

                        counter++;
                    }

                    _gameOptionShips.Add(gameOptionShip);
                }
        
                PlayersUnAddedShips();
               // Console.WriteLine($"SetGame - NextMoveA: {NextMoveByA}");
            }
        }

        public List<Ship> GetAvailableShips()
        {
            // Currently initalizing with 5 default ships
            // var maxSize = 0;
            // if (_width > _height || _width == _height)
            // {
            //     maxSize = _width;
            // }
            // else if (_width < _height)
            // {
            //     maxSize = _height;
            // }
            //
            var shipFactory = new ShipFactory();

            return shipFactory.GetShips(5);
        }

        private BoardSquareState[,] InitEmptyBoard()
        {
            BoardSquareState[,] board = new BoardSquareState[_width, _height];

            for (var x = 0; x < _width; x++)
            {
                for (var y = 0; y < _height; y++)
                {
                    BoardSquareState squareState = new BoardSquareState();
                    board[x, y] = squareState;
                }
            }

            return board;
        }


        public List<GameShip> GetUnaddedGameShips()
        {
            return NextMoveByA ? PlayerAUnAddedGameShips : PlayerBUnAddedGameShips;
        }

        private void PlayersUnAddedShips()
        {
            // Reset unadded ship lists for game init from db
            PlayerAUnAddedGameShips = new List<GameShip>();
            PlayerBUnAddedGameShips = new List<GameShip>();

            // Add ships to Unadded ship lists
            foreach (var gameShip in PlayerAGameShips.Where(gameShip => !IsShipOnBoard(_boardPlayerA, gameShip)))
            {
                
                PlayerAUnAddedGameShips.Add(gameShip);
            }

            foreach (var gameShip in PlayerBGameShips.Where(gameShip => !IsShipOnBoard(_boardPlayerB, gameShip)))
            {
                PlayerBUnAddedGameShips.Add(gameShip);
            }

            // Change NextMove boolean in case of loading the game and there is not all ships added to the board.
            if (PlayerAUnAddedGameShips.Count == 0)
            {
                NextMoveByA = false;
            }

            if (PlayerBUnAddedGameShips.Count == 0)
            {
                NextMoveByA = true;
            }
        }

        private bool IsShipOnBoard(BoardSquareState[,] board, GameShip gameShip)
        {
            for (var row = 0; row < board.GetLength(0); row++)
            {
                for (var col = 0; col < board.GetLength(1); col++)
                {
                    if (board[row, col].GameShipOnBoardNr == gameShip.GameShipOnBoard)
                    {
                        
                        return true;
                    }
                }
            }
            
            return false;
        }

        public IEnumerable<GameShip> GetGameShips(bool current)
        {
            if (current)
            {
                return NextMoveByA ? new List<GameShip>(PlayerAGameShips) : new List<GameShip>(PlayerBGameShips);
            }

            return NextMoveByA ? new List<GameShip>(PlayerBGameShips) : new List<GameShip>(PlayerAGameShips);
        }

        public List<GameShip> GetSunkenGameShips(bool current)
        {
            List<GameShip> playerASunkenGameShips = PlayerAGameShips.Where(aGameShip => aGameShip.IsSunken).ToList();

            List<GameShip> playerBSunkenGameShips = PlayerBGameShips.Where(bGameShip => bGameShip.IsSunken).ToList();

            if (current)
            {
                return NextMoveByA ? playerASunkenGameShips : playerBSunkenGameShips;
            }

            return NextMoveByA ? playerBSunkenGameShips : playerASunkenGameShips;
        }

        public Player GetPlayer(bool currentPlayer)
        {
           // Console.WriteLine($"GetPlayer, Currentplayer: {currentPlayer}, NextMoveByA: {NextMoveByA}");
            if (currentPlayer)
            {
                return NextMoveByA ? _playerA : _playerB;
            }

            return NextMoveByA ? _playerB : _playerA;
        }

        public List<BoardSquareState[,]> GetBoards(bool getOpponentSecretBoard = false, bool bothBoardsPublic = false)
        {
            var boards = new List<BoardSquareState[,]>();
            var res = new BoardSquareState[_width, _height];
            var resOpponent = new BoardSquareState[_width, _height];

            if (NextMoveByA)
            {
                Array.Copy(_boardPlayerA, res, _boardPlayerA.Length);
                if (bothBoardsPublic)
                {
                    Array.Copy(_boardPlayerB, resOpponent, _boardPlayerB.Length);
                }
            }
            else
            {
                Array.Copy(_boardPlayerB, res, _boardPlayerB.Length);
                if (bothBoardsPublic)
                {
                    Array.Copy(_boardPlayerA, resOpponent, _boardPlayerA.Length);
                }
            }

            boards.Add(res);
            if (bothBoardsPublic)
            {
                boards.Add(resOpponent);
            }

            if (getOpponentSecretBoard)
            {
                boards.Add(GetOpponentSecretBoard());
            }

            return boards;
        }

        private BoardSquareState[,] GetOpponentSecretBoard()
        {
            var res = InitEmptyBoard();
            var opponentBoard = NextMoveByA ? _boardPlayerB : _boardPlayerA;
            for (var i = 0; i < opponentBoard.GetLength(0); i++)
            {
                for (var j = 0; j < opponentBoard.GetLength(1); j++)
                {
                    if (!(opponentBoard[i, j].GameShipOnBoardNr > 0 && opponentBoard[i, j].Bomb == 0))
                    {
                        res[i, j] = opponentBoard[i, j];
                    }
                    else
                    {
                        res[i, j].Bomb = opponentBoard[i, j].Bomb;
                        res[i, j].GameShipOnBoardNr = 0;
                    }
                }
            }

            return res;
        }

        public (bool[] successed, PlayerBoardState? playerBoardState) MakeMove(int x, int y)
        {
            //Console.WriteLine($"X: {x}, Y: {y}");
            //Console.WriteLine($"Make move - NextMoveA: {NextMoveByA}");
            var isHit = false;
            PlayerBoardState playerBoardState;
            if (NextMoveByA)
            {
                if (Validator.CanPutBombOnBoard(_boardPlayerB, x, y))
                {
                    _boardPlayerB[x, y].Bomb = MoveCounter;
                    playerBoardState = SetSerializedBoardSquareStates(_boardPlayerB, _playerB);
                    _playerBBoardStates.Add(playerBoardState);
                    
                    // Check if Next move after hit is Same player and hit was made
                    if (_game.GameOption.ENextMoveAfterHit == ENextMoveAfterHit.SamePlayer &&  _boardPlayerB[x, y].GameShipOnBoardNr > 0)
                    {
                        isHit = true;
                    }
                }
                else
                {
                    return (new[]{false, false}, null)!;
                }
            }
            else
            {
                if (Validator.CanPutBombOnBoard(_boardPlayerA, x, y))
                {
                    _boardPlayerA[x, y].Bomb = MoveCounter;
                    playerBoardState = SetSerializedBoardSquareStates(_boardPlayerA, _playerA);
                    _playerABoardStates.Add(playerBoardState);
                    
                    // Check if Next move after hit is Same player and hit was made
                    if (_game.GameOption.ENextMoveAfterHit == ENextMoveAfterHit.SamePlayer &&  _boardPlayerB[x, y].GameShipOnBoardNr > 0)
                    {
                        isHit = true;
                    }
                }
                else
                {
                    return (new[]{false, false}, null)!;
                }
            }

            if (AreShipsSunken())
            {
                return (new[]{true, true}, playerBoardState);
            }

            // If NextMove After hit isnt same player and hit was made or no hit then we change next move
            if (!(_game.GameOption.ENextMoveAfterHit == ENextMoveAfterHit.SamePlayer && isHit) || !isHit)
            {
                NextMoveByA = !NextMoveByA;
            }
            
            // Console.WriteLine($"nextMoveA-makemoveend {NextMoveByA}");
            MoveCounter++;
            return (new[]{true, false}, playerBoardState);
        }

        private bool AreShipsSunken()
        {
            List<GameShip> playerGameShips = NextMoveByA ? PlayerBGameShips : PlayerAGameShips;
            BoardSquareState[,] board = NextMoveByA ? _boardPlayerB : _boardPlayerA;
            var sunkenShipCounter = 0;
            foreach (var gameShip in playerGameShips)
            {
                if (gameShip.IsSunken)
                {
                    sunkenShipCounter++;
                    continue;
                }

                var hitCounter = 0;
                for (var col = 0; col < board.GetLength(0); col++)
                {
                    for (var row = 0; row < board.GetLength(1); row++)
                    {
                        if (board[col, row].GameShipOnBoardNr == gameShip.GameShipOnBoard && board[col, row].Bomb > 0)
                        {
                            hitCounter++;
                        }
                    }
                }

                // Console.WriteLine(hitCounter);
                if (hitCounter != gameShip.Size) continue;
                gameShip.IsSunken = true;
               // Console.WriteLine("gameship is sunken: " + gameShip.IsSunken);
                sunkenShipCounter++;
            }

            return sunkenShipCounter == playerGameShips.Count;
        }

        // Function for detecting is game over. Meant to be used after loading the game from db.
        public bool IsGameOver()
        {
            // Reverse NextMoveByA, to get right result from board where last move was made. After Game setup from db, game changes NextMoveByA to next player...
            NextMoveByA = !NextMoveByA;
            if (AreShipsSunken())
            {
                return true;
            }
            //If it was false, reverse back NextMoveByA!
            NextMoveByA = !NextMoveByA;

            return false;
        }

        public (bool isSuccess, PlayerBoardState? playerBoardState)AddShips(GameShip gameShip, int x, int y, bool isHorizontal)
        {
            //Console.WriteLine($"Addships X: {x}, Y: {y}");
            //Validate Ships
            var canPutShipOnBoard = _validator.CanPutShipOnBoard(NextMoveByA ? _boardPlayerA : _boardPlayerB, x, y,
                gameShip, isHorizontal);

            if (!canPutShipOnBoard)
            {
                return (false, null);
            }

            PlayerBoardState playerBoardState;
            //Put ships on board
            if (isHorizontal)
            {
                for (var col = x; col < x + gameShip.Size; col++)
                {
                    if (NextMoveByA)
                    {
                        _boardPlayerA[col, y].GameShipOnBoardNr = gameShip.GameShipOnBoard;
                    }
                    else
                    {
                        _boardPlayerB[col, y].GameShipOnBoardNr = gameShip.GameShipOnBoard;
                    }
                }
            }
            else
            {
                for (var row = y; row < y + gameShip.Size; row++)
                {
                    if (NextMoveByA)
                    {
                        _boardPlayerA[x, row].GameShipOnBoardNr = gameShip.GameShipOnBoard;
                    }
                    else
                    {
                        _boardPlayerB[x, row].GameShipOnBoardNr = gameShip.GameShipOnBoard;
                    }
                }
            }

            if (NextMoveByA)
            {
                PlayerAUnAddedGameShips.Remove(gameShip);
                playerBoardState = SetSerializedBoardSquareStates(_boardPlayerA, _playerA);
                _playerABoardStates.Add(playerBoardState);
            }
            else
            {
                PlayerBUnAddedGameShips.Remove(gameShip);
                playerBoardState = SetSerializedBoardSquareStates(_boardPlayerB, _playerB);
                _playerBBoardStates.Add(playerBoardState);
            }
            // Update how mony ships player has, if player has all ships on board then other player will put ships etc..

            switch (NextMoveByA)
            {
                case true when PlayerAUnAddedGameShips.Count == 0:
                case false when PlayerBUnAddedGameShips.Count == 0:
                    NextMoveByA = !NextMoveByA;
                    break;
            }

            return (true, playerBoardState);
        }

        public IEnumerable<GameOptionShip> GetGameOptionShips()
        {
            return _gameOptionShips;
        }

        public static IEnumerable<Game> LoadSavedGames(DbContextOptions<ApplicationDbContext> dbOptions)
        {
            
            using var db = new ApplicationDbContext(dbOptions);
            db.Database.Migrate();

            return db.Games.OrderByDescending(g => g.GameId)
                .Include(g => g.PlayerA)
                .ThenInclude(p => p.PlayerBoardStates)
                .Include(g => g.PlayerB)
                .ThenInclude(p => p.PlayerBoardStates)
                .ToList();
        }

        public void LoadGame(int id, DbContextOptions<ApplicationDbContext> dbOptions)
        {
            using var db = new ApplicationDbContext(dbOptions);
            db.Database.Migrate();
            foreach (var dbGame in db.Games
                .Where(g => g.GameId == id)
                .Include(g => g.GameOption)
                .ThenInclude(g => g.GameOptionShips)
                .ThenInclude(s => s.Ship)
                .Include(g => g.PlayerA)
                .Include(g => g.PlayerA.GameShips)
                .Include(g => g.PlayerA.PlayerBoardStates.
                    OrderBy(s => s.CreatedAt))
                .Include(g => g.PlayerB)
                .Include(g => g.PlayerB.GameShips)
                .Include(g => g.PlayerB.PlayerBoardStates
                    .OrderBy(s => s.CreatedAt)))
            {

                SetGameFromDb(dbGame);
            }
        }

        private void SetGameFromDb(Game game)
        {
            _game = game;
            _width = game.GameOption.BoardWidth;
            _height = game.GameOption.BoardHeight;
            _playerA = game.PlayerA;
            _playerB = game.PlayerB;
            
            _gameOptionShips = game.GameOption.GameOptionShips.ToList();
            
            _ships = new List<Ship>();
            foreach (var gameOptionShip in game.GameOption.GameOptionShips)
            {
                _ships.Add(gameOptionShip.Ship);
            }

            
            PlayerAGameShips = _playerA.GameShips.ToList();
            PlayerBGameShips = _playerB.GameShips.ToList();
            

            
            if (game.PlayerA.PlayerBoardStates.Count > 0)
            {
                _boardPlayerA = GetBoardStateFromJson(game.PlayerA.PlayerBoardStates.Last());
                _playerABoardStates = game.PlayerA.PlayerBoardStates.ToList();
            }
            
            if (game.PlayerB.PlayerBoardStates.Count > 0)
            {
                _boardPlayerB = GetBoardStateFromJson(game.PlayerB.PlayerBoardStates.Last());
                _playerBBoardStates = game.PlayerB.PlayerBoardStates.ToList();
            }

            PlayersUnAddedShips();
            // Set bombs amount and next move
            var maxPlayerA = GetMaxBomb(_boardPlayerB);
            var maxPlayerB = GetMaxBomb(_boardPlayerA);
            
            if (maxPlayerA == 0 && maxPlayerB == 0)
            {
                return;
            }
            SetNextMove(maxPlayerA, maxPlayerB);
            // Console.WriteLine(MoveCounter);
            
        }

        private void SetNextMove(int maxPlayerA, int maxPlayerB)
        {
            //Set movecounter
            MoveCounter = maxPlayerA > maxPlayerB ? maxPlayerA : maxPlayerB;
            MoveCounter++;
            
            if (_game.GameOption.ENextMoveAfterHit == ENextMoveAfterHit.OtherPlayer)
            {
                NextMoveByA = MoveCounter % 2 != 0;
                
            }else if (_game.GameOption.ENextMoveAfterHit == ENextMoveAfterHit.SamePlayer)
            {   // If GameOption.ENextMoveAfterHit is Same player we check if last move was hit.
                // If player A movecounter is bigger then need to check if last move was hit
                if (maxPlayerA > maxPlayerB)
                {
                    if (IsLastMoveHit(_boardPlayerB, maxPlayerA))
                    {
                        NextMoveByA = true;
                    }
                    else
                    {
                        NextMoveByA = false;
                    }
                }
                else
                {   //If player B movecounter is bigger then need to check if last move was hit.
                    if (IsLastMoveHit(_boardPlayerA, maxPlayerB))
                    {
                        NextMoveByA = false;
                    }
                    else
                    {
                        NextMoveByA = true;
                    }

                }
            }
        }

        private bool IsLastMoveHit(BoardSquareState[,] board, int lastMoveNr)
        {
            for (var i = 0; i < board.GetLength(0); i++)
            {
                for (var j = 0; j < board.GetLength(1); j++)
                {
                    if (board[i, j].Bomb == lastMoveNr && board[i, j].GameShipOnBoardNr > 0)
                    {
                        return true;
                    }
                }
            }
            
            return false;
        }

        private static int GetMaxBomb(BoardSquareState[,] board)
        {
            var max = 0;
            for (var i = 0; i < board.GetLength(0); i++)
            {
                for (var j = 0; j < board.GetLength(1); j++)
                {
                    if (board[i, j].Bomb > max)
                    {
                        max = board[i, j].Bomb;
                    }
                }
            }

            return max;
        }


        public void SaveGame(DbContextOptions<ApplicationDbContext> dbOptions)
        {
            using var db = new ApplicationDbContext(dbOptions);
            // Console.WriteLine("Deleting DB");
            //db.Database.EnsureDeleted();
            // Console.WriteLine("Migrating DB");
            //db.Database.Migrate();
            //
            foreach (var ship in _ships.Where(ship => !db.Ships.Any(s => s.Size == ship.Size && s.Name == ship.Name)))
            {
                db.Ships.Add(ship);
            }
            

            if (_game.GameId == 0)
            {
                db.Players.Add(_playerA);
                db.Players.Add(_playerB);
                db.Games.Add(_game);
                db.GameOptions.Add(_game.GameOption);
                db.SaveChanges();
            }
            
            
            
            foreach (var aBoardState in _playerABoardStates.Where(aBoardState => aBoardState.PlayerBoardStateId == 0))
            {
                if (aBoardState.PlayerBoardStateId == 0)
                {
                    var player = db.Players.First(p => p.PlayerId == _playerA.PlayerId);
                    aBoardState.Player = player;
                    db.PlayerBoardStates.Add(aBoardState);
                }
                
            }

            foreach (var bBoardState in _playerBBoardStates.Where(bBoardState => bBoardState.PlayerBoardStateId == 0))
            {
                if (bBoardState.PlayerBoardStateId == 0)
                {
                    var player = db.Players.First(p => p.PlayerId == _playerB.PlayerId);
                    bBoardState.Player = player;
                    db.PlayerBoardStates.Add(bBoardState);
                }
                
            }
            
            
            foreach (var gameOptionShip in _gameOptionShips)
            {
                if (gameOptionShip.ShipId == 0)
                {
                    var dbShip = db.Ships.First(s =>
                        s.Size == gameOptionShip.Ship.Size && s.Name == gameOptionShip.Ship.Name);
                    gameOptionShip.Ship = dbShip;
                    db.GameOptionShips.Add(gameOptionShip);
                }
               
            }

            foreach (var playerAGameShip in PlayerAGameShips)
            {
                if (playerAGameShip.GameShipId == 0)
                {
                    db.GameShips.Add(playerAGameShip);
                }
            }

            foreach (var playerBGameShip in PlayerBGameShips)
            {
                if (playerBGameShip.GameShipId == 0)
                {
                    db.GameShips.Add(playerBGameShip);
                }
                
            }
            
            db.SaveChanges();
        }

        private PlayerBoardState SetSerializedBoardSquareStates(BoardSquareState[,] boardSquareState, Player player)
        {
           
            BoardSquareState[][] newBoardState = new BoardSquareState[_width][];
            for (var i = 0; i < newBoardState.Length; i++)
            {
                newBoardState[i] = new BoardSquareState[_height];
            }

            for (var x = 0; x < _width; x++)
            {
                for (var y = 0; y < _height; y++)
                {
                    newBoardState[x][y] = boardSquareState[x, y];
                }
            }

            var jsonOptions = new JsonSerializerOptions()
            {
                WriteIndented = true
            };
            var jsonBoardSquareState = JsonSerializer.Serialize(newBoardState, jsonOptions);
            
            var newPlayerBoardState = new PlayerBoardState()
            {
                Player = player,
                BoardSquareState = jsonBoardSquareState

            };
            return newPlayerBoardState;
        }

        private BoardSquareState[,] GetBoardStateFromJson(PlayerBoardState boardState)
        {
            var playerBoardState = JsonSerializer.Deserialize<BoardSquareState[][]>(boardState.BoardSquareState);

            BoardSquareState[,] board = new BoardSquareState[_game.GameOption.BoardWidth, _game.GameOption.BoardHeight];

            for (var x = 0; x < _game.GameOption.BoardWidth; x++)
            {
                for (var y = 0; y < _game.GameOption.BoardHeight; y++)
                {
                    board[x, y] = playerBoardState![x][y];
                }
            }

            return board;
        }
    }
}