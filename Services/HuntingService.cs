using System;
using System.Collections.Generic;
using System.Linq;
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
    public class HuntingService : IHostedService, IDisposable
    {
        private int executionCount = 0;
        private readonly ILogger<HuntingService> _logger;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IHubContext<HuntHub> _hubContext;
        private Timer _timer;
       

        public HuntingService(ILogger<HuntingService> logger, IServiceScopeFactory scopeFactory, IHubContext<HuntHub> hubcontext)
        {
            _logger = logger;
            _scopeFactory = scopeFactory;
            _hubContext = hubcontext;

        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("HuntingService running");
            _timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromSeconds(10));
            return Task.CompletedTask;
        }

        
        private async void DoWork(object state)
        {
            var count = Interlocked.Increment(ref executionCount);
            _logger.LogInformation("Hunting service is working. Count: {count}", count);

            using (var scope = _scopeFactory.CreateScope())
            {
                var services = scope.ServiceProvider;
        
                using (var context = new ApplicationDbContext(
                    services.GetRequiredService<
                        DbContextOptions<ApplicationDbContext>>())
                )

                {
                    var HuntingPlayers = context.HuntingPlayers
                        .Include(si => si.SpeciesIndividual)
                        .Include(gp => gp.PlayerResource)
                        .ThenInclude(pr => pr.Player)
                        .ThenInclude(p => p.Tile)
                        .ThenInclude(t => t.TileSpecieses)
                        .ThenInclude(ts => ts.Species)
                        .Include(u => u.PlayerResource)
                        .ThenInclude(ui => ui.Player)
                        .ThenInclude(uii => uii.PlayerItems)
                        .ThenInclude(uiii => uiii.Item)
                        .ToList();
                    
                    
                    foreach (var hp in HuntingPlayers)
                    {
                        if (hp.SpeciesIndividual == null)
                        { 
                            var huntingProficiency = 1;
                            var huntingSpeed = 1;
                            foreach (var pi in hp.PlayerResource.Player.PlayerItems)
                            {
                                if (pi.Item.Resource == hp.PlayerResource.Resource)
                                {
                                    if (huntingProficiency < pi.Item.ItemEfficiency)
                                    {
                                        huntingProficiency = pi.Item.ItemEfficiency;
                                    }
                                }
                            }

                            var siList = context.SpeciesIndividuals.Where(si => si.TileSpecies.TileId == hp.PlayerResource.Player.TileId).ToList();
                            var random = new Random();
                            var luckyF = siList[random.Next(0, siList.Count()-1)];
                            var chance = helper.GetRandomNumber(10, 1, 0.4);
                            var amount = (int)Math.Ceiling((double) siList.Count / 100);
                            if (amount > 10)
                            {
                                amount = 10;
                            }

                            if (chance == amount)
                            {
                                //Denne koden er bra!!
                                if (hp.PlayerResource.Player.SignalRConnectionId != null)
                                {
                                    await _hubContext.Clients.Client(hp.PlayerResource.Player.SignalRConnectionId)
                                        .SendAsync("ReceiveHunted",
                                            "Hunted " + luckyF.IndividualName + " the " + luckyF.Species.SpeciesName, luckyF.Id);
                                }

                                hp.SpeciesIndividual = luckyF;
                            }

                        }
                    }
                    await context.SaveChangesAsync();
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