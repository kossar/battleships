using System.Collections.Generic;
using System.Linq;
using DAL;
using Domain;
using Microsoft.EntityFrameworkCore;

namespace GameEngine
{
    public class ShipFactory
    {
        private readonly List<Ship> _defaultShips = new List<Ship>();

        public List<Ship> GetShips(int maxSize)
        {
            GenerateDefaultShips(maxSize);
            
            return _defaultShips;
        }
        
        
        private void GenerateDefaultShips(int maxSize)
        {
            for (var i = 1; i <= maxSize; i++)
            {
                Ship ship = new Ship
                {
                    Name = i switch
                    {
                        1 => "Patrol",
                        2 => "Cruiser",
                        3 => "Submarine",
                        4 => "Battleship",
                        5 => "Carrier",
                        _ => "XXL_Carrier" + $" {i}"
                    },
                    Size = i
                };

                _defaultShips.Add(ship);
            }
        }
        
    }
}