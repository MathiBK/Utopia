using System;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using utopia.Models;

namespace utopia.Data
{
    public class TribeDbInitializer
    {
        public static void Initialize(ApplicationDbContext context, bool dev, IWebHostEnvironment env)
        {
            if (dev)
            {
                context.Database.EnsureCreated();
            }

            
            var tribeCount = 3;
            
            string[] names = File.ReadAllLines(Path.Combine(env.WebRootPath, "assets", "names.txt"));
            var rand = new Random();
            
            var list = context.Tiles.Where(t => t.TileType.tileTypeName != "OCEAN" && t.TileType.tileTypeName != "LAKE").ToList();
            
            for (int i = 0; i < tribeCount; i++)
            {
                var t = new Tribe();
                t.Name = names[rand.Next(0, names.Length)];
                t.Moral = 50;
                var villageName = t.Name + " Village";
                var tile = list[rand.Next(0, list.Count)];
                var village = new Village(tile, t, villageName);
                t.Village = village;
                tile.Village = t.Village;
                tile.VillageHere = true;
                context.Tiles.Update(tile);
                context.Villages.Add(village);
                context.Tribes.Add(t);
            }

            context.SaveChangesAsync();
        }
    }
}