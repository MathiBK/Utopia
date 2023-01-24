using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using utopia.Data;
using utopia.Hubs;
using utopia.Models;

namespace utopia
{
    public class helper
    {
        public static int PopulationDerivative(int x, double capacity, double rate, int population)
        {
            var p = PopulationFunction(x, capacity, rate, population);
            return (int) (rate * p * (1-(p/capacity)));
        }

        public static double PopulationFunction(int t, double capacity, double rate, int initialPop)
        {
            return capacity / (1 + ((capacity - initialPop) / initialPop) * Math.Pow(Math.E, -rate * t));
        }
        
        public static int GetRandomNumber(int max, int min, double probabilityPower = 0.5)
        {
            var randomizer = new Random();
            var randomDouble = randomizer.NextDouble();

            var result = Math.Floor(min + (max + 1 - min) * (Math.Pow(randomDouble, probabilityPower)));
            return (int) result;
        }

        public static double normalDist(int mean, int stdDev)
        {
            Random rand = new Random(); //reuse this if you are generating many
            double u1 = 1.0-rand.NextDouble(); //uniform(0,1] random doubles
            double u2 = 1.0-rand.NextDouble();
            double randStdNormal = Math.Sqrt(-2.0 * Math.Log(u1)) *
                                   Math.Sin(2.0 * Math.PI * u2); //random normal(0,1)
            
            return mean + stdDev * randStdNormal; //random normal(mean,stdDev^2)
        }

        public static void playerDeath(ApplicationDbContext context, Player player, IHubContext<HuntHub> hubContext)
        {
            var p = context.Players.Where(pla => pla.Id == player.Id).Include(pla => pla.PlayerResources).ThenInclude(pla => pla.Resource)
                .Include(pla => pla.PlayerItems).ThenInclude(pi => pi.Item).SingleOrDefault();
            var tr = context.Tribes.Single(t => t.Id == p.TribeId);
            var tl = context.Tiles.Single(t => t.Id == p.TileId);
            var vl = context.Villages.Single(v => v.TribeId == tr.Id);
            var tlNew = context.Tiles.Single(t => t.Id == vl.TileId);
            tl.TilePlayers.Remove(p);
            p.Tile = tlNew;
            p.Hp = 100;
            tlNew.TilePlayers.Add(p);
            
            var hp = context.HuntingPlayers.SingleOrDefault(g => g.PlayerResource.Player == p);
            if (hp != default)
            {
                context.HuntingPlayers.Remove(hp);
            }
            
            var gp = context.GatheringPlayers.SingleOrDefault(g => g.PlayerResource.Player == p);
            if (gp != default)
            {
                context.GatheringPlayers.Remove(gp);
            }
            
            foreach (var pr in p.PlayerResources)
            {
                if (pr.Resource.ResourceName == "Water" || pr.Resource.ResourceName == "Food")
                {
                    pr.Amount = 200;
                }
                else
                {
                    pr.Amount = 0;
                }
            }

            foreach (var pi in p.PlayerItems)
            {
                pi.ItemQuantity = 0;
            }
            
            context.Tiles.Update(tlNew);
            context.Tiles.Update(tl);
            context.Players.Update(p);
            context.SaveChanges();
            
            if (p.Connected)
            {
                hubContext.Clients.Client(p.SignalRConnectionId).SendAsync("PlayerDeath", "ting");
                
                hubContext.Clients.Client(player.SignalRConnectionId)
                    .SendAsync("UpdateCurrentTile", tlNew.Id);
                
                Dictionary<string, int> resDict = new Dictionary<string, int>();
                foreach (var res in p.PlayerResources)
                {
                    resDict.Add(res.Resource.ResourceName, res.Amount); 
                }
                var test = JsonSerializer.Serialize(resDict);
                hubContext.Clients.Client(player.SignalRConnectionId)
                    .SendAsync("RefreshResources", test);
                
                
                var pInfoDict = new Dictionary<string, int>();
                pInfoDict.Add("Attack", player.Attack);
                pInfoDict.Add("Hp", player.Hp);
                var jsonPinfo = JsonSerializer.Serialize(pInfoDict);
                Console.WriteLine(jsonPinfo);
                hubContext.Clients.Client(player.SignalRConnectionId)
                    .SendAsync("UpdatePlayerInfo", jsonPinfo);
                
                
                Dictionary<string, int> itemDict = new Dictionary<string, int>();
                foreach (var item in p.PlayerItems)
                {
                    itemDict.Add(item.Item.ItemName, item.ItemQuantity); 
                }
                var dictJson = JsonSerializer.Serialize(itemDict);
                hubContext.Clients.Client(player.SignalRConnectionId)
                    .SendAsync("RefreshItems", dictJson);
            }
        }
    }
}