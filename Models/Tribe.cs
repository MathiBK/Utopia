using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace utopia.Models
{
    public class Tribe
    {
        public Tribe()
        {
            TribePlayers = new List<Player>();
            TribeItems = new List<TribeItem>();
            TribeResources = new List<TribeResource>();
        }
        
        public Tribe(Village village, int tribeMoral, string name)
        {
            Village = village;
            Name = name;
            Moral = tribeMoral;
            TribePlayers = new List<Player>();
            TribeItems = new List<TribeItem>();
            TribeResources = new List<TribeResource>();
        }

        public int Id { get; set; }
        public Village Village { get; set; }
        public int Moral { get; set; }
        public string Name { get; set; }
        public List<Player> TribePlayers { get; set; }
        public List<TribeItem> TribeItems { get; set; }
        public List<TribeResource> TribeResources { get; set; }
    }
}