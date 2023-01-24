using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using utopia.Models;

namespace utopia.Data
{
    public class AnimalDbInitializer
    {
        public static void Initialize(ApplicationDbContext context, bool dev, IWebHostEnvironment env)
        {
            if (dev)
            {
                context.Database.EnsureCreated();
            }


            context.Species.AddRange(new List<Species>
            {
                new Species("Sheep", "This guy got lots of wool and some meat", 20, false, 10, 20, 0, 1),
                new Species("Cow", "This guy got a lot of meat", 20, false, 10, 20, 1, 5),
                new Species("Bear", "This guy is dangerous", 20, true, 30, 50, 20, 40),
                new Species("Tiger", "This guy is very dangerous", 20, true, 30, 50, 20, 40)
            });
            context.SaveChanges();

            var species = context.Species.ToList();
            var allTiles = context.Tiles
                .Include(tile => tile.TileSpecieses)
                .ThenInclude(tileSpecieses => tileSpecieses.SpeciesIndividuals)
                .ToList();
            
            var f = env.WebRootPath;
            string namePath = Path.Combine(f, "assets", "names.txt");
            string[] names = File.ReadAllLines(namePath);


            // Adding animals
            Console.WriteLine("Adding animals...");
            foreach (var tile in allTiles)
            {
                var random = new Random();
                tile.AnimalCapacity = random.Next(tile.TileType.MinPop, tile.TileType.MaxPop);
                for (var j = 0; j < species.Count; j++)
                {
                    var addedAmount = 0;
                    if (tile.AnimalCapacity/species.Count < 10)
                    {
                        addedAmount = tile.AnimalCapacity;
                    }
                    else
                    {
                        if (species[j].SpeciesType)
                        {
                            addedAmount = (int) Math.Floor(random.Next(10, tile.AnimalCapacity/species.Count)*0.25);
                        }
                        else
                        {
                            addedAmount = random.Next(10, tile.AnimalCapacity/species.Count);
                        }
                    }
                    
                    var ts = new TileSpecies(species[j], addedAmount);
                    for (var z = 0; z < addedAmount; z++)
                    {
                        var name = names[random.Next(0, names.Length)];
                        var si = new SpeciesIndividual(ts, ts.Species, name, "Very cool",
                            random.Next(species[j].HpLow, species[j].HpHigh),
                            random.Next(species[j].AttackLow, species[j].AttackHigh));
                        ts.SpeciesIndividuals.Add(si);
                    }
                    tile.TileSpecieses.Add(ts);
                }
                context.Tiles.Update(tile);
            }
            context.SaveChanges();
        }
    }
}