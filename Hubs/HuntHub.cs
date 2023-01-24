using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using utopia.Data;

namespace utopia.Hubs
{
    public class HuntHub : Hub
    {

        private readonly IServiceProvider _db;
        public HuntHub(IServiceProvider db)
        {
            _db = db;
        }
        public async Task SendMessage(string user, string message)
        {
            await Clients.All.SendAsync("ReceiveMessage", user, message);
        }

        public async Task NotifyHunted(string connectionId, string message)
        {
            await Clients.User(connectionId).SendAsync("ReceiveHunted", message);
        }
        
        public async Task RefreshResources(string connectionId)
        {
            await Clients.User(connectionId).SendAsync("RefreshResources");
        }



        public override Task OnConnectedAsync()
        {
         
            using var scope = _db.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            Console.WriteLine("Connected!");
            var name = Context.User.Identity.Name;
           
            var player = dbContext.Players.SingleOrDefault(u => u.PlayerName == name);
                
            if (player != default)
            {
                player.SignalRConnectionId = Context.ConnectionId;
                player.Connected = true;
                dbContext.SaveChanges();
            }
            
            Clients.All.SendAsync("GetTribe");
            
               
            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception exception)
        { 
            using var scope = _db.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var name = Context.User.Identity.Name;
           
            var player = dbContext.Players.SingleOrDefault(u => u.PlayerName == name);
                
            if (player != default)
            {
                player.SignalRConnectionId = null;
                player.Connected = false;
                dbContext.SaveChanges();
            }
            
            Clients.All.SendAsync("GetTribe");
            
            return base.OnDisconnectedAsync(exception);
        }
    }
}