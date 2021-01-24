using System;
using System.Data.SqlTypes;
using Domain;
using Domain.Enums;

namespace GameEngine
{
    public class Validator
    {
        private readonly EShipsCanTouch _shipsCanTouch;

        public Validator(EShipsCanTouch shipsCanTouch)
        {
            _shipsCanTouch = shipsCanTouch;
        }

        public static bool CanPutBombOnBoard(BoardSquareState[,] board, int x, int y)
        {
            if (x < 0 || y < 0 || x > board.GetLength(0) || y > board.GetLength(1))
            {
                return false;
            }

            return board[x, y].Bomb == 0;
        }
        public bool CanPutShipOnBoard(BoardSquareState[,] board, int x, int y, GameShip gameShip, bool isHorizontal)
        {

            if (isHorizontal)
            {
                if (!(board.GetLength(0) >= x + gameShip.Size))
                {
                    return false;
                }
            }
            else
            {
                if (!(board.GetLength(1) >= y + gameShip.Size))
                {
                    return false;
                }
            }

            if (!NotOverlapOtherShip(board, x, y, gameShip, isHorizontal))
            {
                return false;
            }

            if (_shipsCanTouch == EShipsCanTouch.Corner)
            {
                return !SidesNotTouching(board, x, y, gameShip, isHorizontal);
            }
            return _shipsCanTouch != EShipsCanTouch.No || (SidesNotTouching(board, x, y, gameShip, isHorizontal) && CornersNotTouching(board, x, y, gameShip, isHorizontal));
        }

        private static bool NotOverlapOtherShip(BoardSquareState[,] board, int x, int y, GameShip gameShip, bool isHorizontal)
        {
            if (isHorizontal)
            {
                for (var col = x; col < x + gameShip.Size; col++)
                {
                    if (board[col, y].GameShipOnBoardNr != 0)
                    {
                        return false;
                    }
                }
            }
            else
            {
                for (var row = y; row < y + gameShip.Size; row++)
                {
                    if (board[x, row].GameShipOnBoardNr != 0)
                    {
                        return false;
                    }   
                }
            }
            return true;
        }

        private static bool SidesNotTouching(BoardSquareState[,] board, int x, int y, GameShip gameShip, bool isHorizontal)
        {
            if (isHorizontal)
            {
                if (x > 0)
                {
                    if (board[x - 1, y].GameShipOnBoardNr != 0)
                    {
                        //Console.WriteLine("board[x - 1, y].GameShipOnBoardNr != 0");
                        return false;
                    }
                }

                if (board.GetLength(0) - (x + gameShip.Size) > 0)
                {
                    if (board[x + gameShip.Size, y].GameShipOnBoardNr != 0)
                    {
                        //Console.WriteLine("board[x + gameShip.Size, y].GameShipOnBoardNr != 0");
                        return false;
                    }
                }
                for (var col = x; col < gameShip.Size + x; col++)
                {
                    if (y > 0)
                    {
                        if (board[col, y - 1].GameShipOnBoardNr != 0)
                        {
                            //Console.WriteLine("board[col, y - 1].GameShipOnBoardNr != 0");
                            return false;
                        }
                    }

                    
                    if (board.GetLength(0) - 1 - (y + gameShip.Size) <= 0) continue;
                    if (board[col, y + 1].GameShipOnBoardNr != 0)
                    {
                        //Console.WriteLine("board[col, y + 1].GameShipOnBoardNr != 0");
                        return false;
                    }
                    // //????
                    // if (board.GetLength(1) - (y + gameShip.Size) > 0)
                    // {
                    //     if (board[x, y + gameShip.Size].GameShipOnBoardNr != 0)
                    //     {
                    //         Console.WriteLine("board[x, y + gameShip.Size].GameShipOnBoardNr != 0");
                    //         return false;
                    //     }
                    // }
                    
                }
            }
            else
            {
                // Vertical ship check
                if (y > 0)
                {
                    if (board[x, y - 1].GameShipOnBoardNr != 0)
                    {
                        //Console.WriteLine("board[x, y - 1].GameShipOnBoardNr != 0");
                        return false;
                    }
                }
                if (board.GetLength(1) - (y + gameShip.Size) > 0)
                {
                    if (board[x, y + gameShip.Size].GameShipOnBoardNr != 0)
                    {
                        //Console.WriteLine("board[x, y + gameShip.Size].GameShipOnBoardNr != 0");
                        return false;
                    }
                }
                for (var row = y; row < gameShip.Size + y; row++)
                {
                    if (x > 0)
                    {
                        if (board[x - 1, row].GameShipOnBoardNr != 0)
                        {
                            //Console.WriteLine("board[x - 1, row].GameShipOnBoardNr != 0");
                            return false;
                        }
                    }

                    if (board.GetLength(1) - 1 - x <= 0) continue;
                    if (board[x + 1, row].GameShipOnBoardNr != 0)
                    {
                        //Console.WriteLine("board[x + 1, row].GameShipOnBoardNr != 0");
                        return false;
                    }
                }
            }

            return true;
        }

        private static bool CornersNotTouching(BoardSquareState[,] board, int x, int y, GameShip gameShip,  bool isHorizontal)
        {
            if (x > 0 && y > 0)
            {
                if (board[x - 1, y - 1].GameShipOnBoardNr != 0)
                {
                    //Console.WriteLine("board[x - 1, y - 1].GameShipOnBoardNr != 0");
                    return false;
                }
            }
            
            switch (isHorizontal)
            {
                case true:
                {
                    if (x > 0 && board.GetLength(1) - 1 - y > 0)
                    {
                        if (board[x - 1, y + 1].GameShipOnBoardNr != 0)
                        {
                            //Console.WriteLine("board[x - 1, y + 1].GameShipOnBoardNr != 0");
                            return false;
                        }
                    }
                
                    if (board.GetLength(0) - (x + gameShip.Size) > 0 && y > 0)
                    {
                        if (board[x + gameShip.Size, y - 1].GameShipOnBoardNr != 0)
                        {
                            //Console.WriteLine("board[x + gameShip.Size, y - 1].GameShipOnBoardNr != 0");
                            return false;
                        }
                    }

                    //?
                    if (board.GetLength(0) - (x + gameShip.Size) > 0 && board.GetLength(1) - 1 - y > 1)
                    {
                        if (board[x + gameShip.Size, y + 1].GameShipOnBoardNr != 0)
                        {
                            //Console.WriteLine("board[x + gameShip.Size, y + 1].GameShipOnBoardNr != 0");
                            return false;
                        }
                    }

                    break;
                }
                case false:
                {
                    if (y > 0 && board.GetLength(0) - 1 - x > 0)
                    {
                        if (board[x + 1, y - 1].GameShipOnBoardNr != 0)
                        {
                            //Console.WriteLine("board[x + 1, y - 1].GameShipOnBoardNr != 0");
                            return false;
                        }
                    }

                    if (x > 0 && board.GetLength(1) - (y + gameShip.Size) > 0)
                    {
                        if (board[x - 1, y + gameShip.Size].GameShipOnBoardNr != 0)
                        {
                            //Console.WriteLine("board[x - 1, y + gameShip.Size].GameShipOnBoardNr != 0");
                            return false;
                        }
                    }

                    if (x + 1 <= board.GetLength(0) - 1 && board.GetLength(1) - (y + gameShip.Size) > 0)
                    {
                        Console.WriteLine(x);
                        if (board[x + 1, y + gameShip.Size].GameShipOnBoardNr != 0)
                        {
                            //Console.WriteLine("board[x + 1, y + gameShip.Size].GameShipOnBoardNr != 0");
                            return false;
                        }
                    }

                    break;
                }
            }

            return true;
        }
        
    }
}