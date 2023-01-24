using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using utopia.Helper;

namespace utopia.Models
{
    public class Tile
    {
        public Tile()
        {
            TileResources = new List<TileResource>();
            TileSpecieses = new List<TileSpecies>();
            PlayerSeenTiles = new List<PlayerSeenTile>();
            TilePlayers = new List<Player>();
        }

        public int Id { get; set; }
        public bool VillageHere { get; set; }
        public Village Village { get; set; }
        public int AnimalCapacity { get; set; }
        public TileType TileType { get; set; }

        //Alt for laging av map
        public int HexCordX { get; set; }
        public int HexCordY { get; set; }
        public int TileTypeId { get; set; }
        
        
        public List<TileResource> TileResources { get; set; }
        public List<TileSpecies> TileSpecieses { get; set; }
        public List<Player> TilePlayers { get; set; }
        public List<PlayerSeenTile> PlayerSeenTiles { get; set; }
    }
}