using System;
using System.Collections.Generic;
using System.Linq;
using Domain;
using GameEngine;

namespace GameConsoleUI
{
    public static class BattleShipsConsoleUi
    {
        private static readonly string[] Alphabet = new string[26];

        public static bool IsShipHorizontal { get; private set; } = true;

        public static GameShip DrawShipList(List<GameShip> ships, string? name)
        {
            var ship = new GameShip();
            var shipIndex = 0;
            var userChoice = "";
            var shipNotSelected = true;
            do
            {
                Console.Clear();
                Console.WriteLine();

                Console.WriteLine();
                for (var i = 0; i < ships.Count; i++)
                {
                    if (shipIndex == i)
                    {
                        Console.BackgroundColor = ConsoleColor.DarkCyan;
                    }

                    Console.WriteLine($"{i + 1}) " + ships[i].Name);
                    Console.ResetColor();
                }

                Console.WriteLine();
                if (name != null)
                {
                    Console.Write($"{name} select ship > ");
                }
                else
                {
                    Console.Write("Select ship > ");
                }
                
                Console.Write(userChoice);
                var key = Console.ReadKey();


                switch (key.Key)
                {
                    case ConsoleKey.DownArrow:
                    case ConsoleKey.Tab:
                    {
                        shipIndex++;
                        if (shipIndex > ships.Count - 1)
                        {
                            shipIndex = 0;
                        }

                        userChoice = (shipIndex + 1).ToString();
                        break;
                    }
                    case ConsoleKey.UpArrow:
                    {
                        shipIndex--;
                        if (shipIndex < 0)
                        {
                            shipIndex = ships.Count - 1;
                        }

                        userChoice = (shipIndex + 1).ToString();
                        break;
                    }
                    case ConsoleKey.Enter:
                    {
                        ship = ships[shipIndex];
                        shipNotSelected = false;
                        Console.Clear();
                        break;
                    }
                }
            } while (shipNotSelected);

            return ship;
        }

        public static void GameOverUi(BattleShips battleShips)
        {
            var exit = true;
            do
            {
                Console.Clear();
                DrawBoard(battleShips, (0, 0), false, false, 0);
                Console.BackgroundColor = ConsoleColor.Red;
                Console.WriteLine("GAME OVER");
                Console.ResetColor();
                Console.WriteLine($"{battleShips.GetPlayer(true).Name} won!");

                Console.WriteLine("Press Enter to continue..");
                var key = Console.ReadKey();
                if (key.Key == ConsoleKey.Enter)
                {
                    exit = false;
                }
            } while (exit);
            
        }
        private static void DrawBoard(
            BattleShips battleShips,
            (int x, int y) position,
            bool displayOneBoard,
            bool dynamicDisplay,
            int size)
        {
            var boards = new List<BoardSquareState[,]>();
            switch (displayOneBoard)
            {
                case false when !dynamicDisplay:
                    boards = battleShips.GetBoards(false, true);
                    break;
                case true:
                    boards = battleShips.GetBoards();
                    break;
                default:
                {
                    if (dynamicDisplay)
                    {
                        boards = battleShips.GetBoards(true);
                    }

                    break;
                }
            }
            var currentPlayerBoard = boards[0];
            BoardSquareState[,] opponentBoard = new BoardSquareState[0, 0];
            if (!displayOneBoard)
            {
                opponentBoard = boards[1];
            }

            FillAlphabet(currentPlayerBoard.GetLength(0));


            const string space = "     ";
            var width = currentPlayerBoard.GetUpperBound(0) + 1;
            var height = currentPlayerBoard.GetUpperBound(1) + 1;
            

            var positions = new List<int>();
            if (displayOneBoard)
            {
                positions = ShipPositions(IsShipHorizontal ? position.x : position.y, size);
            }


            DrawColumnLetters(width);
            if (!displayOneBoard)
            {
                // Draw second currentPlayerBoard
                Console.Write(space);
                DrawColumnLetters(width);
            }

            //New Line
            Console.WriteLine();
            DrawBoardHorizontalLines(width);
            if (!displayOneBoard)
            {
                // Draw second currentPlayerBoard
                Console.Write(space);
                DrawBoardHorizontalLines(width);
            }

            // New Line 
            Console.WriteLine();

            // Write rows
            for (var rowIndex = 0; rowIndex < height; rowIndex++)
            {
                if (!displayOneBoard)
                {
                    // Write opponent board rows
                    WriteRows(rowIndex, width, size, dynamicDisplay, position, positions, opponentBoard, battleShips.GetSunkenGameShips(false));
                    // Write currentPlayerBoard rows
                    Console.Write(space);
                    WriteRows(rowIndex, width, size, false, position, positions, currentPlayerBoard, battleShips.GetSunkenGameShips(true));
                }
                else
                {
                    WriteRows(rowIndex, width, size, dynamicDisplay, position, positions, currentPlayerBoard, battleShips.GetSunkenGameShips(true));
                }

                Console.WriteLine();
                // Write opponent board row lines
                DrawBoardHorizontalLines(width);

                if (!displayOneBoard)
                {
                    // Write currentPlayer board horizontal lines
                    Console.Write(space);
                    DrawBoardHorizontalLines(width);
                }

                Console.WriteLine();
            }
        }

        private static void WriteRows(
            int rowIndex,
            int width,
            int size,
            bool dynamicDisplay,
            (int x, int y) position,
            List<int> positions,
            BoardSquareState[,] board,
            List<GameShip> sunkenGameShips)
        {
            // If row number is one digit, then extra space in front of digit
            Console.Write(rowIndex < 9 ? $" {rowIndex + 1} |" : $"{rowIndex + 1} |");

            // Write rows with elements
            for (var colIndex = 0; colIndex < width; colIndex++)
            {
                if (dynamicDisplay)
                {
                    
                    // If size == 0 then placing bombs on currentPlayerBoard
                    if (size == 0)
                    {
                        if (position.x == colIndex && position.y == rowIndex)
                        {
                            Console.BackgroundColor = ConsoleColor.DarkCyan;
                        }
                    }
                    // If size != 0 then placing ships to currentPlayerBoard
                    else
                    {
                        if (IsShipHorizontal && positions.Contains(colIndex) && position.y == rowIndex)
                        {
                            Console.BackgroundColor = ConsoleColor.DarkCyan;
                        }
                        else if (!IsShipHorizontal && position.x == colIndex && positions.Contains(rowIndex))
                        {
                            Console.BackgroundColor = ConsoleColor.DarkCyan;
                        }
                    }
                }


                Console.Write($" {SquareState(board[colIndex, rowIndex], sunkenGameShips)} ");
                Console.ResetColor();
                Console.Write("|");
            }
        }

        private static List<int> ShipPositions(int startIndex, int size)
        {
            List<int> sizes = new();
            for (var i = startIndex; i < startIndex + size; i++)
            {
                sizes.Add(i);
            }

            return sizes;
        }

        private static void DrawColumnLetters(int width)
        {
            //First space to column letters
            Console.Write("   ");
            //Write column letters
            for (int i = 0; i < width; i++)
            {
                Console.Write("  " + Alphabet[i] + " ");
            }
        }

        private static void DrawBoardHorizontalLines(int width)
        {
            //Space for column line
            Console.Write("   ");
            //Write currentPlayerBoard upper line 
            for (var colIndex = 0; colIndex < width; colIndex++)
            {
                Console.Write("+---");
            }

            //Write last + for line last column end
            Console.Write("+");
        }

        public static (int x, int y) BoardControl(
            BattleShips battleShips,
            int size,
            bool displayOneBoard)
        {
            var board = battleShips.GetBoards()[0];
            var validationFail = false;
            var userArrowInput = true;
            var makeMove = true;
            var x = 0;
            var y = 0;
            string keyEntry = "";
            string userChoice = "";
            do
            {
                Console.Clear();
                //Console.WriteLine("MoveCounter: " + battleShips.MoveCounter);
                DrawBoard(battleShips, (x, y), displayOneBoard, true, size);
                if (validationFail)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("");
                    Console.WriteLine("There was a problem with your input, please try again!");
                    Console.ResetColor();
                    Console.WriteLine("");
                    validationFail = !validationFail;
                }

                WritePlayerName(battleShips, displayOneBoard);
                WriteMinMaxInstruction(board, displayOneBoard);

                if (userArrowInput)
                {
                    keyEntry = $"{Alphabet[x]},{y + 1}";
                    Console.WriteLine(keyEntry);
                }
                else
                {
                    Console.WriteLine(userChoice);
                }

                var key = Console.ReadKey();

                switch (key.Key)
                {
                    case ConsoleKey.RightArrow:
                    {
                        x++;
                        if (IsShipHorizontal && displayOneBoard && x + size == board.GetLength(0) + 1)
                        {
                            x = 0;
                        } else if (x == board.GetLength(0))
                        {
                            x = 0;
                        }

                        userArrowInput = true;
                        break;
                    }
                    case ConsoleKey.LeftArrow:
                    {
                        x--;
                        if (IsShipHorizontal && displayOneBoard && x == -1)
                        {
                            x = board.GetLength(0) - size;
                        } else if (x == -1)
                        {
                            x = board.GetLength(0) - 1;
                        }

                        userArrowInput = true;
                        break;
                    }
                    case ConsoleKey.DownArrow:
                    {
                        y++;
                        if (!IsShipHorizontal && displayOneBoard && y + size == board.GetLength(1) + 1)
                        {
                            y = 0;
                        } else if (y == board.GetLength(1))
                        {
                            y = 0;
                        }

                        userArrowInput = true;
                        break;
                    }
                    case ConsoleKey.UpArrow:
                    {
                        y--;
                        if (!IsShipHorizontal && displayOneBoard && y == - 1)
                        {
                            y = board.GetLength(1) - size;
                        } else if (y == -1)
                        {
                            y = board.GetLength(1) - 1;
                        }

                        userArrowInput = true;
                        break;
                    }
                    case ConsoleKey.Enter:
                    {
                        if (!userArrowInput)
                        {
                            if (ValidateBomb(userChoice, board))
                            {
                                var value = userChoice.Split(",");

                                var valX = value[0].Trim().ToUpper();
                                for (var i = 0; i < Alphabet.Length; i++)
                                {
                                    if (valX == Alphabet[i])
                                    {
                                        x = i;
                                        break;
                                    }
                                }

                                y = int.Parse(value[1]) - 1;
                            }
                            else
                            {
                                userChoice = "";
                                validationFail = true;
                                break;
                            }
                        }

                        makeMove = false;
                        break;
                    }
                    case ConsoleKey.Escape:
                    {
                        x = 100;
                        y = 100;
                        makeMove = false;
                        break;
                    }
                    case ConsoleKey.Backspace:
                    {
                        if (userArrowInput)
                        {
                            userChoice = keyEntry;
                        }

                        userArrowInput = false;
                        if (userChoice != "")
                        {
                            userChoice = userChoice.Remove(userChoice.Length - 1);
                        }

                        break;
                    }
                    case ConsoleKey.Spacebar:
                    {
                        IsShipHorizontal = !IsShipHorizontal;
                        break;
                    }
                    default:
                    {
                        userArrowInput = false;
                        x = 0;
                        y = 0;
                        userChoice += key.KeyChar.ToString().ToUpper().Trim();
                        break;
                    }
                }
            } while (makeMove);

            return (x, y);
        }


        private static bool ValidateBomb(string userChoice, BoardSquareState[,] board)
        {
            if (!userChoice.Contains(","))
            {
                Console.WriteLine("Contains comma");
                return false;
            }

            var userValue = userChoice.Split(',');
            var x = userValue[0].Trim().ToUpper();
            if (x.Length != 1 || !Alphabet.Contains(x))
            {
                return false;
            }

            var isNumber = int.TryParse(userValue[1].Trim(), out var y);

            if (!isNumber || y < 0 || y > board.GetLength(1) - 1)
            {
                return false;
            }

            return true;
        }

        private static void FillAlphabet(int boardWidth)
        {
            var counter = 0;
            for (var i = 'A'; i <= 'Z'; i++)
            {
                if (counter == boardWidth)
                {
                    break;
                }

                Alphabet[counter] = i.ToString();
                counter++;
            }
        }

        public static void WriteBoardStartInstruction()
        {
            Console.WriteLine("Upper left corner is (A,1)");
        }

        public static void WriteMinMaxInstruction(BoardSquareState[,] board, bool displayOneBoard)
        {
            var width = Alphabet[board.GetUpperBound(0)];
            var height = board.GetUpperBound(1) + 1;
            Console.WriteLine("Separate coordinates with comma! (,) Example move: C,5");
            if (displayOneBoard)
            {
                Console.WriteLine("To add ship, insert only starting position.");
                Console.WriteLine("Or use arrow keys and press enter to insert ship.");
                Console.WriteLine("Rotate ship with space.");
            }
            else
            {
                Console.WriteLine("Or aim your bomb with keyboard arrows and press Enter for shot!");
            }

            Console.Write($"Give Coordinates A - {width} & 1 - {height} >");
        }

        private static void WritePlayerName(BattleShips battleShips, bool oneBoard)
        {
            if (oneBoard)
            {
                Console.WriteLine(battleShips.GetPlayer(true).Name + " add your ship to the board.");
            }
            else
            {
                Console.WriteLine("Press Esc to quit game!");
                Console.WriteLine(battleShips.GetPlayer(true).Name + " place bomb on " + battleShips.GetPlayer(false).Name + " ship!");
            }
        }

        public static string GetPlayerName(string whichPlayer)
        {
            Console.Clear();
            Console.WriteLine($"Press enter for default: {whichPlayer}");
            Console.Write($"Please enter {whichPlayer} name: ");

            string name = Console.ReadLine() ?? "";
            if (name.Length < 1)
            {
                //Console.WriteLine("Please enter at least 1 character");
                name = whichPlayer;
            }
            

            return name;
        }

        private static string SquareState(BoardSquareState squareState, IEnumerable<GameShip> sunkenGameShips)
        {
            if (squareState.GameShipOnBoardNr == 0 && squareState.Bomb == 0)
            {
                return " ";
            }

            if (squareState.GameShipOnBoardNr == 0 && squareState.Bomb > 0)
            {
                //return squareState.Bomb.ToString();
                return "X";
            }

            if (squareState.GameShipOnBoardNr != 0 && squareState.Bomb > 0)
            {
                return sunkenGameShips.Any(sunkenGameShip => sunkenGameShip.GameShipOnBoard == squareState.GameShipOnBoardNr && sunkenGameShip.IsSunken) ? "S" : "H";
            }

            //return squareState.GameShipOnBoardNr.ToString();
            return "B";
        }
    }
}