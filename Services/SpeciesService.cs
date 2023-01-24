using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using utopia.Data;
using utopia.Models;

namespace utopia.Services
{
    public class SpeciesService : IHostedService, IDisposable
    {
        
        private int executionCount = 0;
        private readonly ILogger<ResourceService> _logger;
        private readonly IWebHostEnvironment _env;
        private readonly IServiceScopeFactory _scopeFactory;
        private Timer _timer;

        public SpeciesService(ILogger<ResourceService> logger, IServiceScopeFactory scopeFactory, IWebHostEnvironment env)
        {
            _logger = logger;
            _scopeFactory = scopeFactory;
            _env = env;
        }
        
        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("SpeciesService running");
            _timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromSeconds(3600));

            return Task.CompletedTask;
        }
        
        private async void DoWork(object state)
        {
            var count = Interlocked.Increment(ref executionCount);

            _logger.LogInformation("Species service is working. Count: {count}", count);
            Stopwatch sw = new Stopwatch();
            sw.Start();
            
            using (var scope = _scopeFactory.CreateScope())
            {
                var services = scope.ServiceProvider;
        
                using (var context = new ApplicationDbContext(
                    services.GetRequiredService<DbContextOptions<ApplicationDbContext>>())
                )
                {
                    var allTiles = context.Tiles
                            .Include(tile => tile.TileSpecieses)
                            .ThenInclude(tileSpecies => tileSpecies.SpeciesIndividuals)
                            .ThenInclude(tileSpecies => tileSpecies.Species)
                            .ToList();
                    
                    foreach (var t in allTiles)
                    {
                        var totalCarn = 0;
                        var totalHerb = 0;
                        var differentHerbSpecies = 0;
                        var differentCarnSpecies = 0;
                        var desc = "Very cool";
                        foreach (var ts in t.TileSpecieses)
                        {
                            ts.Amount = ts.SpeciesIndividuals.Count;
                            if (ts.Species.SpeciesType)
                            {
                                if (ts.Amount > 1)
                                {
                                    differentCarnSpecies++;
                                }
                                totalCarn += ts.Amount;
                            }
                            else
                            {
                                if (ts.Amount > 1)
                                {
                                    differentHerbSpecies++;
                                }
                                totalHerb += ts.Amount;
                            }
                        }
                        var rand = new Random();
                        foreach (var ts in t.TileSpecieses)
                        {
                            int popIncrease = 0;
                            if (ts.Species.SpeciesType)
                            {
                                 popIncrease = helper.PopulationDerivative(1, totalHerb*0.2/differentCarnSpecies*rand.Next(7, 13)/10, 0.5, ts.Amount);
                            }
                            else
                            {
                                var animalPrecentage = 0;
                                if (totalHerb != 0 && differentHerbSpecies != 0)
                                {
                                    animalPrecentage = ts.Amount / totalHerb;

                                    popIncrease = helper.PopulationDerivative(1,
                                        (double) t.AnimalCapacity / differentHerbSpecies * rand.Next(7, 13) / 10,
                                        0.5 * rand.Next(8, 12) / 10, ts.Amount);

                                    if (totalCarn < totalHerb / differentHerbSpecies && ts.SpeciesIndividuals.Count != 0)
                                    {
                                        var animalsEaten = totalCarn * animalPrecentage * rand.Next(5, 12) / 10;
                                        for (var i = 0; i < animalsEaten; i++)
                                        {
                                            if (ts.SpeciesIndividuals.Count == 0)
                                            {
                                                break;
                                            }
                                            var randNum = rand.Next(0, ts.SpeciesIndividuals.Count-1);
                                            context.SpeciesIndividuals.Remove(ts.SpeciesIndividuals[randNum]);
                                            //ts.SpeciesIndividuals.Remove(ts.SpeciesIndividuals[randNum]);
                                            ts.SpeciesIndividuals.RemoveAt(randNum);
                                        }
                                        context.TileSpecies.Update(ts);
                                    }
                                }
                            }

                            if (popIncrease < 0 && ts.SpeciesIndividuals.Count != 0)
                            {
                                popIncrease *= -1;
                                for (var i = 0; i < popIncrease; i++)
                                {
                                    if (ts.SpeciesIndividuals.Count == 0)
                                    {
                                        break;
                                    }
                                    var randNum = rand.Next(0, ts.SpeciesIndividuals.Count-1);
                                    context.SpeciesIndividuals.Remove(ts.SpeciesIndividuals[randNum]);
                                    //ts.SpeciesIndividuals.Remove(ts.SpeciesIndividuals[randNum]);
                                    ts.SpeciesIndividuals.RemoveAt(randNum);
                                }
                                context.TileSpecies.Update(ts);
                            }
                            else
                            {
                                var f = _env.WebRootPath;
                                string namePath = Path.Combine(f, "assets", "names.txt");
                                string[] names = File.ReadAllLines(namePath);

                                var list = ts.SpeciesIndividuals.OrderBy(s => s.IndividualChadness).ToList();
                                if (list.Count == 0)
                                {
                                    break;
                                }
                                for (var i = 0; i < popIncrease; i++)
                                {
                                    var biasedRandomPick = helper.GetRandomNumber(list.Count()-1, 0);
                                    var theChad = list[biasedRandomPick];
                                    var name = names[rand.Next(0, names.Length)];
                                    var si = new SpeciesIndividual(ts, ts.Species, name, desc,
                                        (int)helper.normalDist(theChad.IndividualHp, 1),
                                        (int)helper.normalDist(theChad.IndividualAttack, 1));
                                    
                                    si.IndividualChadness = (int)helper.normalDist(theChad.IndividualChadness+1, 1);
                                    
                                    if (si.IndividualChadness > 255)
                                    {
                                        si.IndividualChadness = 255;
                                    }
                                    if (si.IndividualChadness < 0)
                                    {
                                        si.IndividualChadness = 0;
                                    }
                                    if (si.IndividualHp < 1)
                                    {
                                        si.IndividualHp = 1;
                                    }
                                    if (si.IndividualAttack < 1)
                                    {
                                        si.IndividualAttack = 1;
                                    }
                                    ts.SpeciesIndividuals.Add(si);
                                }
                            }
                            ts.Amount = ts.SpeciesIndividuals.Count;
                            context.TileSpecies.Update(ts);
                        }
                        context.Tiles.Update(t);
                    }
                    Console.WriteLine("Trying to save...");
                    Stopwatch sw2 = new Stopwatch();
                    sw2.Start();
                    try
                    {
                        await context.SaveChangesAsync();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }

                    Console.WriteLine(sw2.ElapsedMilliseconds/1000);
                    Console.WriteLine("Save complete!");
                    Console.WriteLine(sw.ElapsedMilliseconds/1000);
                }
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Resource service is stopping");

            _timer?.Change(Timeout.Infinite, 0);
            
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}