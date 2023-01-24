using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using utopia.Data;
using utopia.Models;

namespace utopia.Services
{
    public class ResourceService : IHostedService, IDisposable
    {
        private int executionCount = 0;
        private readonly ILogger<ResourceService> _logger;
        private readonly IServiceScopeFactory _scopeFactory;
        private Timer _timer;

        public ResourceService(ILogger<ResourceService> logger, IServiceScopeFactory scopeFactory)
        {
            _logger = logger;
            _scopeFactory = scopeFactory;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("ResourceService running");
            _timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromSeconds(3600));
            
            return Task.CompletedTask;
        }

        private async void DoWork(object state)
        {
            var count = Interlocked.Increment(ref executionCount);
            _logger.LogInformation("Resource service is working. Count: {count}", count);

            using (var scope = _scopeFactory.CreateScope())
            {
            
                var services = scope.ServiceProvider;
        
                using (var context = new ApplicationDbContext(
                    services.GetRequiredService<
                        DbContextOptions<ApplicationDbContext>>())
                )
                {
                    var allTiles = context.Tiles
                        .Include(tile => tile.TileType)
                        .Include(tile => tile.TileResources)
                        .ThenInclude(tileResource => tileResource.Resource)
                        .ToList();
                    
                    foreach (var t in allTiles)
                    {
                        var random = new Random();
                        
                        foreach (var tr in t.TileResources)
                        {
                            if (tr.Resource.ResourceName == "Berries")
                            {
                                tr.Amount += helper.PopulationDerivative(1, tr.ResourceCap, 1, tr.Amount);
                                if (tr.Amount < 0)
                                {
                                    tr.Amount = 0;
                                }
                            }
                        }
                        context.Tiles.Update(t);
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