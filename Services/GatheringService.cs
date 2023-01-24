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
using utopia.Data;
using utopia.Hubs;
using utopia.Models;

namespace utopia.Services
{
    public class GatheringService : IHostedService, IDisposable
    {
        private int executionCount = 0;
        private readonly ILogger<GatheringService> _logger;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IHubContext<HuntHub> _hubContext;
        private Timer _timer;
       

        public GatheringService(ILogger<GatheringService> logger, IServiceScopeFactory scopeFactory, IHubContext<HuntHub> hubcontext)
        {
            _logger = logger;
            _scopeFactory = scopeFactory;
            _hubContext = hubcontext;
            
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("GatheringService running");
            _timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromSeconds(10));
            return Task.CompletedTask;
        }

        
        private async void DoWork(object state)
        {
            var count = Interlocked.Increment(ref executionCount);
            _logger.LogInformation("Gathering service is working. Count: {count}", count);

            using (var scope = _scopeFactory.CreateScope())
            {
                var services = scope.ServiceProvider;
        
                using (var context = new ApplicationDbContext(
                    services.GetRequiredService<
                        DbContextOptions<ApplicationDbContext>>())
                )

                {
                    var gatheringPlayers = context.GatheringPlayers
                        .Include(gp => gp.PlayerResource)
                        .ThenInclude(pr => pr.Player)
                        .ThenInclude(p => p.Tile)
                        .ThenInclude(t => t.TileResources)
                        .ThenInclude(tr => tr.Resource)
                        .Include(u => u.PlayerResource)
                        .ThenInclude(ui => ui.Player)
                        .ThenInclude(uii => uii.PlayerItems)
                        .ThenInclude(uiii => uiii.Item)
                        .ToList();
                    
                    
                    foreach (var pr in gatheringPlayers)
                    {
                        var gatheringProficiency = 1;
                        var gatheringSpeed = 1;
                        foreach (var pi in pr.PlayerResource.Player.PlayerItems)
                        {
                            if (pi.Item.Resource == pr.PlayerResource.Resource)
                            {
                                if (gatheringProficiency < pi.Item.ItemEfficiency)
                                {
                                    gatheringProficiency = pi.Item.ItemEfficiency;
                                }
                            }
                        }

                        var tr = pr.PlayerResource.Player.Tile.TileResources.Single(t => t.Resource == pr.PlayerResource.Resource);
                        PlayerResource prbSingle = context.PlayerResources.Single(p => p.Resource.ResourceName == "Food" && p.Player == pr.PlayerResource.Player);
                        if (pr.PlayerResource.Resource.ResourceName == "Berries")
                        {
                            prbSingle.Amount += gatheringSpeed * gatheringProficiency;
                        }
                        else
                        {
                            pr.PlayerResource.Amount += gatheringSpeed * gatheringProficiency;
                        }
                        

                        if (tr.Amount > 0 && tr.Amount - (gatheringSpeed * gatheringPlayers.Count()) >= 0 )
                        {
                            tr.Amount -= gatheringSpeed * gatheringPlayers.Count();
                        }
                        //Denne koden er bra!!
                        if (pr.PlayerResource.Player.SignalRConnectionId != null)
                        {
                            var playerResources = await context.PlayerResources.Where(p => p.PlayerId == pr.PlayerResource.Player.Id).
                                Include(r => r.Resource).ToListAsync();
                            Dictionary<string, int> resDict = new Dictionary<string, int>();
                            foreach (var res in playerResources)
                            {
                                resDict.Add(res.Resource.ResourceName, res.Amount); 
                            }
                            
                            var test = JsonSerializer.Serialize(resDict);
                        
                        
                            Console.WriteLine(test);
                           
                            await _hubContext.Clients.Client(pr.PlayerResource.Player.SignalRConnectionId)
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