namespace utopia.Models
{
    public class PlayerSeenTile
    {
        public PlayerSeenTile(){}
        public PlayerSeenTile(Player player, Tile tile)
        {
            Player = player;
            Tile = tile;
        }
        public int Id { get; set; }
        public Player Player { get; set; }
        public int PlayerId { get; set; }
        public Tile Tile { get; set; }
        public int TileId { get; set; }
    }
}