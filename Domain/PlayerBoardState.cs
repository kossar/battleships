using System;

namespace Domain
{
    public class PlayerBoardState
    {
        public int PlayerBoardStateId { get; set; }

        public int PlayerId { get; set; }
        public Player? Player { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        //Serialized json
        public string BoardSquareState { get; set; } = null!;
    }
}