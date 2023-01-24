using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace utopia.Models
{
    public class Village
    {
        public Village() { }

        public Village(Tile tile, Tribe tribe, string name)
        {
            Tile = tile;
            Tribe = tribe;
            VillageName = name;
        }

        public int Id { get; set; }
        public int TileId { get; set; }
        public Tile Tile { get; set; }
        public Tribe Tribe { get; set; }
        public int TribeId { get; set; }
        public string VillageName { get; set; }
    }
}