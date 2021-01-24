using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain
{
    public class Game
    {
        public int GameId { get; set; }

        public int GameOptionId { get; set; }
        public GameOption GameOption { get; set; } = null!;

        public string Description { get; set; } = DateTime.Now.ToLongDateString();
        
        public int PlayerAId { get; set; }
        
        public Player PlayerA { get; set; } = null!;

        public int PlayerBId { get; set; }
        
        public Player PlayerB { get; set; } = null!;
    }
}