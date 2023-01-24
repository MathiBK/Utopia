using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace utopia.Models
{
    public class TileType
    {
        public TileType() { }

        public TileType(string name, string desc, double foodMultiplier, int max, int min, int minResStone, int maxResStone, int minResBerries, int maxResBerries, int minResWater, int maxResWater, int minResWood, int maxResWood)
        {
            tileTypeName = name;
            tileTypeDesc = desc;

            FoodMultiplier = foodMultiplier;
            
            MaxPop = max;
            MinPop = min;
            
            MinResStone = minResStone;
            MinResBerries = minResBerries;
            MinResWater = minResWater;
            MinResWood = minResWood;   
            
            MaxResStone = maxResStone;
            MaxResBerries = maxResBerries;
            MaxResWater = maxResWater;
            MaxResWood = maxResWood;
        }
        
        
        public int Id { get; set; }
        public double FoodMultiplier { get; set; }
        public int MaxPop { get; set; }
        public int MinPop { get; set; }
        public int MinResStone { get; set; }
        public int MinResWater { get; set; }
        public int MinResBerries { get; set; }
        public int MinResWood { get; set; }
        public int MaxResStone { get; set; }
        public int MaxResWater { get; set; }
        public int MaxResBerries { get; set; }
        public int MaxResWood { get; set; }
        
        public string tileTypeName { get; set; }
        public string tileTypeDesc { get; set; }
    }
}