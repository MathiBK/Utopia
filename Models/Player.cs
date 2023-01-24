using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace utopia.Models
{
    public class Player
    {
        public Player()
        {
            PlayerItems = new List<PlayerItem>();
            PlayerResources = new List<PlayerResource>();
            PlayerSeenTiles = new List<PlayerSeenTile>();
        }
        
        public Player(string name)
        {
            PlayerItems = new List<PlayerItem>();
            PlayerResources = new List<PlayerResource>();
            PlayerSeenTiles = new List<PlayerSeenTile>();
            PlayerName = name;
            Hp = 100;
            Attack = 1;
        }
        
        public int Id { get; set; }
        public int TileId { get; set; }
        public int Hp { get; set; }
        public int Attack { get; set; }
        public Tile Tile { get; set; }
        public int TribeId { get; set; }
        public Tribe Tribe { get; set; }
        public string PlayerName { get; set; }
        public string SignalRConnectionId { get; set; }
        public bool Connected { get; set; }
        public List<PlayerItem> PlayerItems { get; set; }
        public List<PlayerResource> PlayerResources { get; set; }
        public List<PlayerSeenTile> PlayerSeenTiles { get; set; }
    }
}