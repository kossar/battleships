namespace Domain
{
    public class BoardSquareState
    {
        // GameShip.GameshipID
        public int GameShipOnBoardNr { get; set; }
        public int Bomb { get; set; } // 0 - no bomb, 1..x - bomb placement in numbered order
    }
}