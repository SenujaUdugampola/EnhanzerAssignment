using Enhanzer.API.Models;
using Microsoft.EntityFrameworkCore;

namespace Enhanzer.API.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<LocationDetail> Locations => Set<LocationDetail>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<LocationDetail>(entity =>
        {
            entity.ToTable("Location_Details");
            entity.HasKey(location => location.Id);
            entity.Property(location => location.LocationCode)
                .HasColumnName("Location_Code")
                .HasMaxLength(100)
                .IsUnicode(false)
                .IsRequired();
            entity.Property(location => location.LocationName)
                .HasColumnName("Location_Name")
                .HasMaxLength(255)
                .IsUnicode(false)
                .IsRequired();
        });
    }
}
