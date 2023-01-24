using System.Collections.Generic;
using System.Linq;
using utopia.Helper;
using utopia.Models;

namespace utopia.Data
{
    public class MapDbInitializer
    {
 public static void Initialize(ApplicationDbContext context, bool dev)
        {
            if (dev)
            {
                context.Database.EnsureDeleted();
                context.Database.EnsureCreated();
            }

            

            //Add different materials and biomes

            context.TileTypes.AddRange(new List<TileType>
            {new TileType("MARSH", "So much wet and low!", 3, 0, 0, 50, 100, 0, 30, 6000, 10000, 50, 100),
                new TileType("ICE", "So much wet and high!", 4, 0, 0, 50, 100, 0, 30, 6000, 10000, 50, 100),
                new TileType("OCEAN", "So much salty water", 5,0, 0, 0, 0, 0, 0, 0, 0, 0, 0),
                new TileType("LAKE", "So much non salty water", 1.5,0, 0, 50, 100, 0, 30, 6000, 10000, 50, 100),
                new TileType("BEACH", "So much soft sand", 1.3,300, 100, 100, 300, 0, 30, 500, 1000, 50, 100),
                new TileType("SNOW", "So much snow", 2.5,400, 100, 100, 300, 0, 30, 500, 1000, 50, 100),
                new TileType("TUNDRA", "So much cold dirt", 1.5,300, 200, 100, 300, 0, 30, 500, 1000, 50, 100),
                new TileType("BARE", "So much nothing", 1,200, 100, 100, 300, 0, 30, 500, 1000, 50, 100),
                new TileType("SCORCHED", "So much stone", 2,50, 10, 100, 300, 0, 30, 500, 1000, 50, 100),
                new TileType("TAIGA", "So much snowy trees", 1.5,500, 200, 100, 300, 0, 30, 500, 1000, 50, 100),
                new TileType("SHRUBLAND", "So much shrub", 2.2,600, 300, 100, 300, 0, 30, 500, 1000, 50, 100),
                new TileType("TEMPERATE_DESERT", "So much normal sand", 3,200, 100, 100, 300, 0, 30, 500, 1000, 50, 100),
                new TileType("TEMPERATE_RAIN_FOREST", "So much wet tree", 2,1000, 600, 100, 300, 0, 30, 500, 1000, 50, 100),
                new TileType("TEMPERATE_DECIDUOUS_FOREST", "So much nice tree", 2,1200, 700, 100, 300, 0, 30, 500, 1000, 50, 100),
                new TileType("GRASSLAND", "So much grass", 1,900, 400, 100, 300, 0, 30, 500, 1000, 50, 100),
                new TileType("TROPICAL_RAIN_FOREST", "So much hot trees", 2.5,1600, 1200, 100, 300, 0, 30, 500, 1000, 50, 100),
                new TileType("TROPICAL_SEASONAL_FOREST", "So much changing", 1.3,1400, 1000, 100, 300, 0, 30, 500, 1000, 50, 100),
                new TileType("SUBTROPICAL_DESERT", "So much boring sand", 2.5,200, 100, 100, 300, 0, 30, 500, 1000, 50, 100)
            });
            
            
            context.SaveChanges();
            
            var tileTypes = context.TileTypes.ToList();
            
            //Generate world
            MapGen m = new MapGen(2000, 75400);
            
            foreach (var c in m.Centers)
            {
                var t = new Tile();
                t.HexCordX = c.TileHex.X;
                t.HexCordY = c.TileHex.Y;
                t.TileTypeId = tileTypes.Find(x => x.tileTypeName == c.Biome).Id;
                t.TileType = tileTypes.Find(x => x.tileTypeName == c.Biome);
                
                context.Tiles.Add(t);
            }
            context.SaveChanges();
        }
    }
}