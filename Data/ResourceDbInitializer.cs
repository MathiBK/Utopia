
﻿using System;
 using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
 using utopia.Helper;
 using utopia.Models;

namespace utopia.Data
{
    public class ResourceDbInitializer
    {

        public static void Initialize(ApplicationDbContext context, bool dev)
        {
          
            if (dev)
            {
                context.Database.EnsureCreated();
            }


            
            context.Resources.AddRange(new List<Resource>
            {
                new Resource(1, "Stone", "sick stone dude!", 15),
                new Resource(2, "Water", "holy pogger thats some nice water", 40),
                new Resource(3, "Wood", "maybe you can get some wood in the moning", 30),
                new Resource(4, "Berries", "Nam nam, tastes very nice", 10),
                new Resource(5, "Food", "Nam nam, tastes very nice", 0)
            });
            
            context.SaveChanges();
            
            var resources = context.Resources.ToList();
            var allTiles = context.Tiles
                .Include(tile => tile.TileResources)
                .ThenInclude(tileResource => tileResource.Resource)
                .ToList();
            
            Console.WriteLine("Adding resources...");
            var random = new Random();
            foreach (var tile in allTiles)
            {
                for (var j = 0; j < resources.Count; j++)
                {
                    if (resources[j].ResourceName == "Stone")
                    {
                        var resourceCountAndCap = random.Next(tile.TileType.MinResStone, tile.TileType.MaxResStone);
                        tile.TileResources.Add(new TileResource(resources[j], resourceCountAndCap,resourceCountAndCap));
                    }
                    else if(resources[j].ResourceName == "Wood")
                    {
                        var resourceCountAndCap = random.Next(tile.TileType.MinResWood, tile.TileType.MaxResWood);
                        tile.TileResources.Add(new TileResource(resources[j], resourceCountAndCap,resourceCountAndCap));
                    }
                    else if(resources[j].ResourceName == "Berries")
                    {
                        var resourceCountAndCap = random.Next(tile.TileType.MinResBerries, tile.TileType.MaxResBerries);
                        tile.TileResources.Add(new TileResource(resources[j], resourceCountAndCap,resourceCountAndCap));
                    }
                    else if(resources[j].ResourceName == "Water")
                    {
                        var resourceCountAndCap = random.Next(tile.TileType.MinResWater, tile.TileType.MaxResWater);
                        tile.TileResources.Add(new TileResource(resources[j], resourceCountAndCap,resourceCountAndCap));
                    }
                }
                context.Tiles.Update(tile); 
            }
            context.SaveChanges();
        }
    }
}