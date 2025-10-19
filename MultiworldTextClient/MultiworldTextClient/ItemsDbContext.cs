using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using MultiworldTextClient.Data.Database;

namespace MultiworldTextClient;

public class ItemsDbContext : DbContext
{
    DbSet<ProcessedItem> ProcessedItems { get; set; }
    
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        string dbName = "multiworld.db";

        if (!File.Exists(dbName))
        {
            File.Create(dbName).Dispose();
        }
        
        optionsBuilder.UseSqlite($"Data Source={dbName}");
        
        Database.EnsureCreated();
        Database.Migrate();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<ProcessedItem>(entity =>
        {
            entity.Property(e => e.ItemId).IsRequired();
            entity.Property(e => e.LocationId).IsRequired();
            entity.Property(e => e.TrackerUuid).IsRequired();
        });
    }
}