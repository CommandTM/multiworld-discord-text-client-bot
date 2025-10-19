using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using MultiworldTextClient.Data.Database;

namespace MultiworldTextClient;

public class ItemsDbContext : DbContext
{
    public DbSet<ProcessedItem> ProcessedItems { get; set; }
    public DbSet<TrackedWorld> TrackedWorlds { get; set; }
    
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite($"Data Source=multiworld.db");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<ProcessedItem>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ItemId).IsRequired();
            entity.Property(e => e.LocationId).IsRequired();
            entity.Property(e => e.TrackerUuid).IsRequired();
        });
        
        modelBuilder.Entity<TrackedWorld>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.GuildId).IsRequired();
            entity.Property(e => e.TrackerUuid).IsRequired();
            entity.Property(e => e.BaseUrl).IsRequired();
            entity.Property(e => e.ChannelId).IsRequired();
            entity.Property(e => e.RoomUuid).IsRequired();
        });
    }
}