using Enhanzer.API.Models;
using Microsoft.EntityFrameworkCore;

namespace Enhanzer.API.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<LocationDetail> Locations => Set<LocationDetail>();
    public DbSet<PurchaseBill> PurchaseBills => Set<PurchaseBill>();
    public DbSet<PurchaseBillItem> PurchaseBillItems => Set<PurchaseBillItem>();

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

        modelBuilder.Entity<PurchaseBill>(entity =>
        {
            entity.ToTable("Purchase_Bills");
            entity.HasKey(bill => bill.Id);
            entity.Property(bill => bill.BillNo).HasColumnName("Bill_No").HasMaxLength(30).IsUnicode(false).IsRequired();
            entity.Property(bill => bill.PurchaseDate).HasColumnName("Purchase_Date")
                .HasDefaultValueSql("SYSUTCDATETIME()");
            entity.Property(bill => bill.CreatedBy).HasColumnName("Created_By").HasMaxLength(255).IsUnicode(false);
            entity.Property(bill => bill.CreatedAt).HasColumnName("Created_At")
                .HasDefaultValueSql("SYSUTCDATETIME()");
            entity.HasMany(bill => bill.Items)
                .WithOne(item => item.PurchaseBill)
                .HasForeignKey(item => item.PurchaseBillId);
        });

        modelBuilder.Entity<PurchaseBillItem>(entity =>
        {
            entity.ToTable("Purchase_Bill_Items");
            entity.HasKey(item => item.Id);
            entity.Property(item => item.PurchaseBillId).HasColumnName("Purchase_Bill_Id");
            entity.Property(item => item.ItemName).HasColumnName("Item_Name").HasMaxLength(100).IsUnicode(false).IsRequired();
            entity.Property(item => item.BatchCode).HasColumnName("Batch_Code").HasMaxLength(100).IsUnicode(false).IsRequired();
            entity.Property(item => item.StandardCost).HasColumnName("Standard_Cost").HasPrecision(18, 2);
            entity.Property(item => item.StandardPrice).HasColumnName("Standard_Price").HasPrecision(18, 2);
            entity.Property(item => item.MarginPercent).HasColumnName("Margin_Percent").HasPrecision(5, 2);
            entity.Property(item => item.Quantity).HasColumnName("Quantity").HasPrecision(18, 3);
            entity.Property(item => item.FreeQuantity).HasColumnName("Free_Quantity").HasPrecision(18, 3);
            entity.Property(item => item.DiscountPercent).HasColumnName("Discount_Percent").HasPrecision(5, 2);
            entity.Property(item => item.TotalCost).HasColumnName("Total_Cost")
                .HasComputedColumnSql("(Standard_Cost * Quantity) * (1 - (Discount_Percent / 100))");
            entity.Property(item => item.TotalSelling).HasColumnName("Total_Selling")
                .HasComputedColumnSql("Standard_Price * Quantity");
        });
    }
}
