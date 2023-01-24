using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using utopia.Controllers;
using utopia.Data;
using utopia.Hubs;
using utopia.Models;

namespace utopia.Services
{
    public class FoodService : IHostedService, IDisposable
    {
        private int executionCount = 0;
        private readonly ILogger<FoodService> _logger;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IHubContext<HuntHub> _hubContext;
        private Timer _timer;
       

        public FoodService(ILogger<FoodService> logger, IServiceScopeFactory scopeFactory,  IHubContext<HuntHub> hubcontext)
        {
            _logger = logger;
            _scopeFactory = scopeFactory;
            _hubContext = hubcontext;

        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("FoodService running");
            _timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromSeconds(300));
            return Task.CompletedTask;
        }

        
        private async void DoWork(object state)
        {
            var count = Interlocked.Increment(ref executionCount);
            _logger.LogInformation("Food service is working. Count: {count}", count);

            using (var scope = _scopeFactory.CreateScope())
            {
                var services = scope.ServiceProvider;

                using (var context = new ApplicationDbContext(
                    services.GetRequiredService<
                        DbContextOptions<ApplicationDbContext>>())
                )

                {
                    var players = context.Players
                        .Include(p => p.PlayerResources)
                        .ThenInclude(pr => pr.Resource)
                        .Include(p => p.Tile)
                        .ThenInclude(t => t.TileType)
                        .ToList();

                    foreach (var p in players)
                    {
                        var food = p.PlayerResources.FirstOrDefault(pr => pr.Resource.ResourceName == "Food");
                        var water = p.PlayerResources.FirstOrDefault(pr => pr.Resource.ResourceName == "Water");
                        var consumedFood = (int) Math.Floor(5 * p.Tile.TileType.FoodMultiplier);
                        if (food == default) continue;
                        if (food.Amount >= consumedFood)
                        {
                            food.Amount -= consumedFood;
                            if (p.Hp < 100)
                            {
                                p.Hp += 5;
                                if (p.Hp > 100)
                                {
                                    p.Hp = 100;
                                }
                            }

                        }
                        else if (food.Amount < consumedFood && food.Amount > 0)
                        {
                            p.Hp -= food.Amount;
                            food.Amount = 0;
                        }
                        else
                        {
                            p.Hp -= consumedFood;
                        }
                        if (water == default) continue;
                        if (water.Amount >= consumedFood)
                        {
                            water.Amount -= consumedFood;
                            if (p.Hp < 100)
                            {
                                p.Hp += 5;
                                if (p.Hp > 100)
                                {
                                    p.Hp = 100;
                                }
                            }

                        }
                        else if (water.Amount < consumedFood && water.Amount > 0)
                        {
                            p.Hp -= water.Amount;
                            water.Amount = 0;
                        }
                        else
                        {
                            p.Hp -= consumedFood;
                        }

                        if (p.Hp <= 0)
                        {
                            p.Hp = 0;
                            helper.playerDeath(context, p, _hubContext);
                        }
                        else
                        {
                            context.Players.Update(p);
                        }
                        
                        if (p.Connected)
                        {
                            var playerResources = await context.PlayerResources.Where(play => p.Id == play.PlayerId).
                                Include(r => r.Resource).ToListAsync();
                            Dictionary<string, int> resDict = new Dictionary<string, int>();
                            foreach (var res in playerResources)
                            {
                                resDict.Add(res.Resource.ResourceName, res.Amount); 
                            }
                            
                            var test = JsonSerializer.Serialize(resDict);
                        
                        
                            Console.WriteLine(test);
                           
                            await _hubContext.Clients.Client(p.SignalRConnectionId)
                                .SendAsync("RefreshResources", test);
                            
                            
                        }
                        await context.SaveChangesAsync();


                    }
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