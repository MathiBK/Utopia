using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using utopia.Models;

namespace utopia.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Tile>()
                .HasOne(t => t.Village)
                .WithOne(v => v.Tile)
                .HasForeignKey<Village>(v => v.TileId);

            modelBuilder.Entity<Tile>()
                .HasMany(t => t.TilePlayers)
                .WithOne(p => p.Tile)
                .HasForeignKey(p => p.TileId);

            modelBuilder.Entity<Tile>()
                .HasMany(t => t.PlayerSeenTiles)
                .WithOne(pst => pst.Tile)
                .HasForeignKey(pst => pst.TileId);

            modelBuilder.Entity<Player>()
                .HasMany(p => p.PlayerSeenTiles)
                .WithOne(pst => pst.Player)
                .HasForeignKey(pst => pst.PlayerId);
            
            modelBuilder.Entity<Tribe>()
                .HasOne(t => t.Village)
                .WithOne(v => v.Tribe)
                .HasForeignKey<Village>(v => v.TribeId);

            modelBuilder.Entity<TileSpecies>()
                .HasMany(ts => ts.SpeciesIndividuals)
                .WithOne(si => si.TileSpecies)
                .HasForeignKey(si => si.TileSpeciesId);
            
            modelBuilder.Entity<SpeciesIndividual>()
                .HasOne(si => si.TileSpecies)
                .WithMany(ts => ts.SpeciesIndividuals)
                .HasForeignKey(si => si.TileSpeciesId)
                .OnDelete(DeleteBehavior.ClientSetNull);
            
            modelBuilder.Entity<TileType>()
                .Property(p => p.Id)
                .ValueGeneratedOnAdd();
            
            modelBuilder.UseIdentityColumns();
        }
        
        public DbSet<Resource> Resources { get; set; }
        public DbSet<Tile> Tiles { get; set; }
        public DbSet<Player> Players { get; set; }
        public DbSet<PlayerItem> PlayerItems { get; set; }
        public DbSet<Item> Items { get; set; }
        public DbSet<Species> Species { get; set; }
        public DbSet<SpeciesIndividual> SpeciesIndividuals { get; set; }
        public DbSet<TileType> TileTypes { get; set; }
        public DbSet<Tribe> Tribes { get; set; }
        public DbSet<TribeItem> TribeItems { get; set; }
        public DbSet<Village> Villages { get; set; }
        public DbSet<TileResource> TileResources { get; set; }
        public DbSet<TileSpecies> TileSpecies { get; set; }
        public DbSet<PlayerResource> PlayerResources { get; set; }
        public DbSet<TribeResource> TribeResources { get; set; }
        public DbSet<GatheringPlayers> GatheringPlayers { get; set; }
        public DbSet<ItemCrafting> ItemCrafting { get; set; }
        public DbSet<HuntingPlayers> HuntingPlayers { get; set; }
        public DbSet<PlayerSeenTile> PlayerSeenTiles { get; set; }
    }
}
