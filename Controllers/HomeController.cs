using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using utopia.Data;
using utopia.Helper;
using utopia.Hubs;
using utopia.Models;
using utopia.Models.ViewModels;
using utopia.Services;

namespace utopia.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _db;
        private UserManager<IdentityUser> _um;
        private SignInManager<IdentityUser> _sim;
        private readonly IHubContext<HuntHub> _hubContext;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context, UserManager<IdentityUser> um, SignInManager<IdentityUser> sim, IHubContext<HuntHub> hubContext)
        {
            _logger = logger;
            _db = context;
            _um = um;
            _sim = sim;
            _hubContext = hubContext;
        }

        public IActionResult Index()
        {
            return View();
        }
        
        [Authorize]
        public IActionResult Privacy()
        {
            return View();
        }
        
        [Authorize]
        public IActionResult Game()
        {
            return View();
        }

        [HttpGet]
        public IActionResult CurrentPlayerTile()
        {
            if (_sim.IsSignedIn(User))
            {
                Player player = _db.Players.FirstOrDefault(p => p.PlayerName == User.Identity.Name);
                if (player != default)
                {
                    return Ok(player.TileId);
                }
            }
            return BadRequest();
        }
        
        [Authorize]
        [HttpPost]
        public IActionResult ResGather(string resourceName)
        {
            if (_sim.IsSignedIn(User))
            {
                Player player = _db.Players.FirstOrDefault(p => p.PlayerName == User.Identity.Name);
                if (player != default)
                {
                    Resource rs = _db.Resources.FirstOrDefault(r => r.ResourceName == resourceName);
                    PlayerResource prSingle = _db.PlayerResources.Single(pr => pr.PlayerId == player.Id && pr.Resource == rs);

                    var gp = _db.GatheringPlayers.Include(p => p.PlayerResource).ToList();
                    foreach (var pr in gp)
                    {
                        if (pr.PlayerResource.Player == player)
                        {
                            if (pr.PlayerResource.Resource == rs)
                            {
                                _db.GatheringPlayers.Remove(pr);
                                _db.SaveChangesAsync();
                                return Ok();
                            }
                            _db.GatheringPlayers.Remove(pr);
                            break;
                        }
                    }
                    _db.GatheringPlayers.Add(new GatheringPlayers(prSingle));
                    _db.SaveChangesAsync();
                    return Ok();
                }
            }
            return BadRequest();
        }

        [HttpGet]
        public IActionResult GetResource(string resName)
        {
            if (_sim.IsSignedIn(User))
            {
                Player player = _db.Players.FirstOrDefault(p => p.PlayerName == User.Identity.Name);
                if (player != default)
                {
                    PlayerResource prSingle = _db.PlayerResources.Single(pr =>
                        pr.PlayerId == player.Id && pr.Resource.ResourceName == resName);
                    var resAmount = prSingle.Amount;
                    return Ok(resAmount);
                }
            }
            return BadRequest();
        }
        
        [HttpGet]
        public IActionResult GetItem(string itemName)
        {
            if (_sim.IsSignedIn(User))
            {
                Player player = _db.Players.FirstOrDefault(p => p.PlayerName == User.Identity.Name);
                if (player != default)
                {
                    PlayerItem piSingle = _db.PlayerItems.SingleOrDefault(pi => pi.Player.Id == player.Id && pi.Item.ItemName == itemName);
                    if (piSingle != default)
                    {
                      var itemQuantity = piSingle.ItemQuantity; 
                      return Ok(itemQuantity);  
                    }

                    return Ok(0);
                }
            }
            return BadRequest();
        }

        [HttpGet]
        public IActionResult GetVisibleTiles()
        {
            if (_sim.IsSignedIn(User))
            {
                Player player = _db.Players.Where(p => p.PlayerName == User.Identity.Name).Include(p => p.Tile)
                    .FirstOrDefault();
                if (player != default)
                {
                    List<Tile> visibleTiles = _db.PlayerSeenTiles.Where(pst => pst.Player == player)
                        .Select(pst => pst.Tile).ToList();
                    List<int> visibleTilesId = new List<int>();
                    foreach (var tile in visibleTiles)
                    {
                        visibleTilesId.Add(tile.Id);
                    }
                    return Ok(visibleTilesId);
                }
                Console.WriteLine("Couldn't find player object");
                return BadRequest();
            }
            Console.WriteLine("User not logged in");
            return BadRequest();
        }
        
        [HttpPost]
        public async Task<IActionResult> SaveVisibleTiles()
        {
            if (_sim.IsSignedIn(User))
            {
                Player player = _db.Players.Where(p => p.PlayerName == User.Identity.Name).Include(p => p.Tile)
                    .FirstOrDefault();
                if (player != default)
                {
                    List<Tile> allTiles = _db.Tiles.ToList();
                    List<Tile> visibleTiles = _db.PlayerSeenTiles.Where(pst => pst.Player == player)
                        .Select(pst => pst.Tile).ToList();
                    foreach (var tile in allTiles)
                    {
                        if (Hex.Distance(player.Tile.HexCordX, player.Tile.HexCordY, tile.HexCordX, tile.HexCordY) <= 1)
                        {
                            if (!visibleTiles.Contains(tile))
                            {
                                PlayerSeenTile playerSeenTile = new PlayerSeenTile(player, tile);
                                player.PlayerSeenTiles.Add(playerSeenTile);
                                tile.PlayerSeenTiles.Add(playerSeenTile);
                                player.PlayerSeenTiles.Add(playerSeenTile);
                            }
                        }
                    }
                    _db.Players.Update(player);
                    await _db.SaveChangesAsync();
                    return Ok();
                }
                Console.WriteLine("Couldn't find player object");
                return BadRequest();
            }
            Console.WriteLine("User not logged in");
            return BadRequest();
        }
        
        [HttpPost]
        public async Task<IActionResult> MovePlayer(int hexX, int hexY)
        {
            Console.WriteLine("Attempting move!");
            if (_sim.IsSignedIn(User))
            {
                Player player = _db.Players.Where(p => p.PlayerName == User.Identity.Name).Include(p => p.Tile).ThenInclude(p=> p.TilePlayers)
                    .FirstOrDefault();
                
                var newTile = _db.Tiles.Where(t => t.HexCordX == hexX && t.HexCordY == hexY).Include(t => t.TileType).FirstOrDefault();

            
                if (newTile != default && player != default)
                {
                    //Passe på at man ikke sniker seg inn på ocean eller lake, selv om det er skrudd av i JS
                    if (newTile.TileType.tileTypeName == "OCEAN" || newTile.TileType.tileTypeName == "LAKE")
                    {
                        return BadRequest();
                    }

                    if (Hex.Distance(player.Tile.HexCordX, player.Tile.HexCordY, newTile.HexCordX, newTile.HexCordY) > 1)
                    {
                        return BadRequest();
                    }

                    var playTile = player.Tile;
                    var pList = _db.Tiles.Where(t => t.TilePlayers.Contains(player)).Include(p=> p.TilePlayers).FirstOrDefault();
                    if (pList != default)
                    {
                        player.Tile.TilePlayers.Remove(player);
                        player.Tile = newTile;
                        newTile.TilePlayers.Add(player);
                        player.TileId = newTile.Id;
                        _db.Players.Update(player);
                        _db.Tiles.Update(newTile);
                        _db.Tiles.Update(pList);
                        await _db.SaveChangesAsync();
                        await SaveVisibleTiles();
                        if (player.Connected)
                        {
                            await _hubContext.Clients.Client(player.SignalRConnectionId)
                                .SendAsync("UpdateCurrentTile", newTile.Id);
                            await _hubContext.Clients.Client(player.SignalRConnectionId)
                                .SendAsync("ShowVisibleTiles");
                        }
                        
                        return Ok();
                    }
                }

            }
            Console.WriteLine("Move failed!");
            return BadRequest();
        }

        public IActionResult UITest()
        {
            return View();
        }

        
        [HttpPost]
        public IActionResult HuntConfirmed()
        {
            if (_sim.IsSignedIn(User))
            {
                var player = _db.Players.FirstOrDefault(player1 => player1.PlayerName == User.Identity.Name);
                if (player == default)
                    return BadRequest("player not found");
                var hp = _db.HuntingPlayers.Where(hp => hp.PlayerResource.PlayerId == player.Id).Include(pr=>pr.PlayerResource).Include(hpp => hpp.SpeciesIndividual)
                    .ThenInclude(hppp => hppp.Species).FirstOrDefault();
                if (hp != default)
                {

                    var pAtck = hp.PlayerResource.Player.Attack;
                    var aAtck = hp.SpeciesIndividual.IndividualAttack;
                    _logger.LogInformation(
                        $"Heldige faen. luckyF: {hp.SpeciesIndividual.IndividualName} species: {hp.SpeciesIndividual.Species.SpeciesName}",
                        hp.SpeciesIndividual.IndividualName, hp.SpeciesIndividual.Species.SpeciesName);
                    var pHealth = hp.PlayerResource.Player.Hp;
                    var aHealth = hp.SpeciesIndividual.IndividualHp;
                    while (aHealth > 0 && pHealth > 0)
                    {
                        aHealth -= pAtck;
                        pHealth -= aAtck;

                        _logger.LogInformation($"pHealth: {pHealth} aHealth: {aHealth}", aHealth, pHealth);
                    }

                    var playerFoodResource = _db.PlayerResources.Single(pr =>
                        pr.Resource.ResourceName == "Food" && hp.PlayerResource.Player == pr.Player);
                    if (aHealth <= 0 && !(pHealth <= 0))
                    {
                        playerFoodResource.Amount += hp.SpeciesIndividual.Species.SpeciesDropness;
                        hp.PlayerResource.Player.Hp = pHealth;
                        if (player.SignalRConnectionId != null)
                        {
                            Console.WriteLine(hp.SpeciesIndividual.IndividualName);
                            Console.WriteLine( hp.SpeciesIndividual.Species.SpeciesName);
                            var huntData = "<div class='container'><h2>You hunted " +  hp.SpeciesIndividual.IndividualName + " the " +
                                           hp.SpeciesIndividual.Species.SpeciesName +"</h2></div>";
                            _hubContext.Clients.Client(player.SignalRConnectionId)
                                .SendAsync("AnimalSlain",
                                    huntData);
                            
                            var playerResources = _db.PlayerResources.Where(play => player.Id == play.PlayerId).
                                Include(r => r.Resource).ToList();
                            Dictionary<string, int> resDict = new Dictionary<string, int>();
                            foreach (var res in playerResources)
                            {
                                resDict.Add(res.Resource.ResourceName, res.Amount); 
                            }
                            
                            var test = JsonSerializer.Serialize(resDict);
                        
                        
                            Console.WriteLine(test);
                           
                            _hubContext.Clients.Client(player.SignalRConnectionId)
                                .SendAsync("RefreshResources", test);
                            var pInfoDict = new Dictionary<string, int>();
                            pInfoDict.Add("Attack", player.Attack);
                            pInfoDict.Add("Hp", player.Hp);
                            var jsonPinfo = JsonSerializer.Serialize(pInfoDict);
                            Console.WriteLine(jsonPinfo);
                            _hubContext.Clients.Client(player.SignalRConnectionId)
                                .SendAsync("UpdatePlayerInfo", jsonPinfo);
                            
                        }
                        _db.SpeciesIndividuals.Remove(hp.SpeciesIndividual);
                        _db.SaveChangesAsync();
                        
                       
                    } else if (pHealth <= 0)
                    {
                        player.Hp = 0;
                        if (player.SignalRConnectionId != null)
                        {
                            Console.WriteLine(hp.SpeciesIndividual.IndividualName);
                            Console.WriteLine( hp.SpeciesIndividual.Species.SpeciesName);
                            var huntData = "<div class='container'><h2>You died</h2></div>";
                            _hubContext.Clients.Client(player.SignalRConnectionId)
                                .SendAsync("AnimalSlain",
                                    huntData);
                    
                            helper.playerDeath(_db, player, _hubContext);
                        }
                        _db.SaveChangesAsync();
                    }
                }

                return Ok();
            }

            return BadRequest("Not signed in");
        }
        
        [HttpPost]
        public IActionResult HuntRejected()
        {
            if (_sim.IsSignedIn(User))
            {
                var player = _db.Players.FirstOrDefault(player1 => player1.PlayerName == User.Identity.Name);
                if (player == default)
                    return BadRequest("player not found");
                var huntP = _db.HuntingPlayers.Where(p => p.PlayerResource.Player.Id == player.Id).Include(i => i.SpeciesIndividual).FirstOrDefault();
                huntP.SpeciesIndividual = null;
                _db.Update(huntP);
                _db.SaveChangesAsync();

                return Ok("Hunt rejected");
            }

            return BadRequest("Not signed in");
        }
        
        [HttpGet]
        public IActionResult HuntConfirm(int speciesIndividId)
        {
            if (_sim.IsSignedIn(User))
            {
                Player player = _db.Players.FirstOrDefault(p => p.PlayerName == User.Identity.Name);
                var huntingPlayer = _db.HuntingPlayers
                    .Where(hp => hp.PlayerResource.Player.Id == player.Id)
                    .Include(hp => hp.SpeciesIndividual)
                    .ThenInclude(hppp => hppp.Species)
                    .FirstOrDefault();
                if (huntingPlayer != default)
                {
                    var speciesFromDb = _db.SpeciesIndividuals.Where(i => i.Id == speciesIndividId).Include(s => s.Species).FirstOrDefault();
                    if (speciesFromDb != default)
                    {
                        var individTemp = speciesFromDb;
                        return PartialView("_HuntConfirmPartial", individTemp);
                    }
                }
                
                return Ok();
            }

            return BadRequest();

        }
        
        [HttpGet]
        public IActionResult TileInfo(int id)
        {

            var tileTemp = _db.Tiles.SingleOrDefault(t => t.Id == id);
            if (tileTemp != default)
            {
                var tileType = _db.TileTypes.SingleOrDefault(t => t.Id == tileTemp.TileTypeId);
                if (tileType != default)
                {
                    var tileInfo = new TileInfoViewModel();
                    tileInfo.Tile = tileTemp;
                    tileInfo.TileType = tileType;
                    return PartialView("_TileInformationPartial", tileInfo);
                }
            }
            return BadRequest();
        }

        
        [HttpGet]
        public async Task<IActionResult> GetTribeInfo()
        {
            if (_sim.IsSignedIn(User))
            {

                Player player = await _db.Players.FirstOrDefaultAsync(p => p.PlayerName == User.Identity.Name);
                Tribe playerTribe = await _db.Tribes.Where(t => t.Id == player.TribeId).Include(t => t.TribePlayers)
                    .SingleOrDefaultAsync();
                if (player == default || playerTribe == default)
                    return BadRequest();
                return PartialView("_TribePartial", playerTribe);

            }

            return BadRequest();
        }


        [HttpPost]
        public async Task<IActionResult> ItemCraft(string itemName)
        {
            if (_sim.IsSignedIn(User)) { 
                
                Player player = _db.Players.FirstOrDefault(p => p.PlayerName == User.Identity.Name);
                ItemCrafting ic = _db.ItemCrafting.FirstOrDefault(icc => icc.Item.ItemName == itemName);
                if (player == default || ic == default)
                    return BadRequest();
                var playerResources = _db.PlayerResources.Where(i => i.Player.Id == player.Id).Include(i => i.Resource).ToList();

                var enoughMaterials = true;
                foreach (var r in playerResources)
                {
                    if (r.Resource.ResourceName == "Stone")
                    {
                        if (r.Amount < ic.Resource1Amount)
                        {
                            enoughMaterials = false;
                            return Ok("Not enough");
                        }
                    }
                    if (r.Resource.ResourceName == "Wood")
                    {
                        if (r.Amount < ic.Resource2Amount)
                        {
                            enoughMaterials = false;
                            return Ok("Not enough");
                        }
                    }
                }

                if (enoughMaterials)
                {
                    var list = _db.PlayerItems.Where(i => i.Item.ItemName == itemName && i.Player.Id == player.Id).ToList();
                    if (list.Count != 0)
                    {
     
                        list[0].ItemQuantity += 1;
                        _db.PlayerItems.Update(list[0]);
                        
                        
                    }
                    else
                    {
                        var item = _db.Items.Single(i => i.ItemName == itemName);
                        var pr = new PlayerItem(item, 1, player);
                        player.PlayerItems.Add(pr);
                        _db.PlayerItems.Add(pr);
                        _db.Players.Update(player);
                    }
                    
                    foreach (var r in playerResources)
                    {
                        if (r.Resource.ResourceName == "Stone")
                        {
                            r.Amount -= ic.Resource1Amount;
                        }
                        if (r.Resource.ResourceName == "Wood")
                        {
                            r.Amount -= ic.Resource2Amount;
                        }
                        
                    }
            
                   
                }
                await _db.SaveChangesAsync();
                
                
                if (player.Connected)
                {
                    var playerItems =  _db.PlayerItems.Where(p => p.Player.Id == player.Id).
                        Include(r => r.Item).ToList();
                    Dictionary<string, int> itemDict = new Dictionary<string, int>();
                    foreach (var item in playerItems)
                    {
                        itemDict.Add(item.Item.ItemName, item.ItemQuantity); 
                    }
                            
                    var dictJson = JsonSerializer.Serialize(itemDict);
                            
                    Console.WriteLine(dictJson);
                           
                    await _hubContext.Clients.Client(player.SignalRConnectionId)
                        .SendAsync("RefreshItems", dictJson);

                }
                if (player.Connected)
                {
                    var playerResourcesSignalR = _db.PlayerResources.Where(p => p.PlayerId == player.Id).
                        Include(r => r.Resource).ToList();
                    Dictionary<string, int> resDict = new Dictionary<string, int>();
                    foreach (var res in playerResourcesSignalR)
                    {
                        resDict.Add(res.Resource.ResourceName, res.Amount); 
                    }
                            
                    var test = JsonSerializer.Serialize(resDict);
                        
                        
                    Console.WriteLine(test);
                           
                    await _hubContext.Clients.Client(player.SignalRConnectionId)
                        .SendAsync("RefreshResources", test);
                            
                            
                }
                
                
                return Ok();
            }
            
            return BadRequest("Not signed in");
        }
        
        [HttpPost]
        public IActionResult Hunting(bool start)
        {
            if (_sim.IsSignedIn(User))
            {
                Player player = _db.Players.FirstOrDefault(p => p.PlayerName == User.Identity.Name);
                PlayerResource prSingle = _db.PlayerResources.FirstOrDefault(pr => pr.PlayerId == player.Id && pr.Resource.ResourceName == "Food");
                if (player == default || prSingle == default)
                    return BadRequest("Player or resource not found");
                var hp = _db.HuntingPlayers.Include(hpp => hpp.PlayerResource).ToList();
                var inList = hp.Exists(f => f.PlayerResource.Player.Id == player.Id);
                if (start)
                {
                    if (inList)
                    {
                        return Ok("Player already in list");
                    }
                    _db.HuntingPlayers.Add(new HuntingPlayers(prSingle));
                    _db.SaveChangesAsync();
                    return Ok("Added to hunting list");
                    
                }
                else
                {
                    if (inList)
                    {
                        _db.HuntingPlayers.Remove(hp.Find(f => f.PlayerResource.Player.Id == player.Id));
                        _db.SaveChangesAsync();
                        return Ok("Removed from list");
                    }

                    return Ok("Player not in list");
                }


            }
            return BadRequest("Not signed in");
        }
        
        [HttpGet]
        public IActionResult FetchPossibleHunt()
        {
            if (_sim.IsSignedIn(User))
            {
                Player player = _db.Players.FirstOrDefault(p => p.PlayerName == User.Identity.Name);
                var huntingPlayer = _db.HuntingPlayers
                    .Where(hp => hp.PlayerResource.Player.Id == player.Id)
                    .Include(hp => hp.SpeciesIndividual)
                    .ThenInclude(hppp => hppp.Species)
                    .FirstOrDefault();
                if (huntingPlayer != default)
                {
                    if (huntingPlayer.SpeciesIndividual != null)
                    {
                        var individTemp = huntingPlayer.SpeciesIndividual;
                        return PartialView("_HuntConfirmPartial", individTemp);
                    }

                    return Ok("No animal found yet");
                }


            }
            return BadRequest("Not signed in");
        }
        
        [HttpGet]
        public async Task<IActionResult> FetchPlayerResources()
        {
            if (_sim.IsSignedIn(User))
            {
                Player player = _db.Players.FirstOrDefault(p => p.PlayerName == User.Identity.Name);
                if (player != default)
                {
                    var playerResources = await _db.PlayerResources.Where(p => p.PlayerId == player.Id).
                        Include(r => r.Resource).ToListAsync();
                    Dictionary<string, int> resDict = new Dictionary<string, int>();
                    foreach (var res in playerResources)
                    {
                        resDict.Add(res.Resource.ResourceName, res.Amount); 
                    }
                            
                    var test = JsonSerializer.Serialize(resDict);
                        
                        
                    Console.WriteLine(test);
                    return Ok(test);
                    await _hubContext.Clients.Client(player.SignalRConnectionId)
                        .SendAsync("RefreshResources", test);
                }

                return BadRequest("Player not found");


            }
            return BadRequest("Not signed in");
        }
        
        [HttpGet]
        public async Task<IActionResult> FetchPlayerStats()
        {
            if (_sim.IsSignedIn(User))
            {
                Player player = _db.Players.FirstOrDefault(p => p.PlayerName == User.Identity.Name);
                if (player != default)
                {
                    var pInfoDict = new Dictionary<string, int>();
                    pInfoDict.Add("Attack", player.Attack);
                    pInfoDict.Add("Hp", player.Hp);
                    var jsonPinfo = JsonSerializer.Serialize(pInfoDict);
                    Console.WriteLine(jsonPinfo);
                    return Ok(jsonPinfo);


                }

                return BadRequest("Player not found");


            }
            return BadRequest("Not signed in");
        }

        
        [HttpGet]
        public async Task<IActionResult> FetchPlayerItems()
        {
            if (_sim.IsSignedIn(User))
            {
                Player player = _db.Players.FirstOrDefault(p => p.PlayerName == User.Identity.Name);
                if (player != default)
                {
                    var playerItems = await _db.PlayerItems.Where(p => p.Player.Id == player.Id).
                        Include(r => r.Item).ToListAsync();
                    Dictionary<string, int> itemDict = new Dictionary<string, int>();
                    foreach (var item in playerItems)
                    {
                        itemDict.Add(item.Item.ItemName, item.ItemQuantity); 
                    }
                            
                    var test = JsonSerializer.Serialize(itemDict);
                        
                        
                    Console.WriteLine(test);
                    return Ok(test);
                    await _hubContext.Clients.Client(player.SignalRConnectionId)
                        .SendAsync("RefreshResources", test);
                }

                return BadRequest("Player not found");


            }
            return BadRequest("Not signed in");
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
