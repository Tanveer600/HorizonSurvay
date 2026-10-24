using HorizonSurvey.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace HorizonSurvey.Infrastructure.Persistence;

public sealed class InventoryDbContext(DbContextOptions<InventoryDbContext> options) : DbContext(options)
{
    private static readonly ValueConverter<DateTimeOffset, long> UtcTicksConverter = new(
        value => value.UtcDateTime.Ticks,
        ticks => new DateTimeOffset(ticks, TimeSpan.Zero));

    public DbSet<EquipmentAsset> EquipmentAssets => Set<EquipmentAsset>();
    public DbSet<EquipmentCategory> EquipmentCategories => Set<EquipmentCategory>();
    public DbSet<PooledStock> PooledStocks => Set<PooledStock>();
    public DbSet<Checkout> Checkouts => Set<Checkout>();
    public DbSet<DroneApproval> DroneApprovals => Set<DroneApproval>();
    public DbSet<InventoryMovement> InventoryMovements => Set<InventoryMovement>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<EquipmentAsset>(entity =>
        {
            entity.HasKey(item => item.Id);
            entity.Property(item => item.Name).HasMaxLength(200).IsRequired();
            entity.HasOne(item => item.Category)
                .WithMany()
                .HasForeignKey(item => item.CategoryId)
                .OnDelete(DeleteBehavior.SetNull);
            entity.HasIndex(item => item.CategoryId);
        });

        modelBuilder.Entity<EquipmentCategory>(entity =>
        {
            entity.HasKey(item => item.Id);
            entity.Property(item => item.Name).HasMaxLength(100).IsRequired();
            entity.HasIndex(item => item.Name).IsUnique();
        });

        modelBuilder.Entity<PooledStock>(entity =>
        {
            entity.ToTable("PooledStocks", table =>
                table.HasCheckConstraint(
                    "CK_PooledStock_Quantities",
                    "[TotalQuantity] >= 0 AND [AvailableQuantity] >= 0 AND [AvailableQuantity] <= [TotalQuantity]"));
            entity.HasKey(item => item.Id);
            entity.Property(item => item.Name).HasMaxLength(200).IsRequired();
        });

        modelBuilder.Entity<Checkout>(entity =>
        {
            entity.ToTable("Checkouts", table =>
            {
                table.HasCheckConstraint(
                    "CK_Checkout_Target",
                    "(([EquipmentAssetId] IS NOT NULL AND [PooledStockId] IS NULL AND [Quantity] = 1) OR ([EquipmentAssetId] IS NULL AND [PooledStockId] IS NOT NULL AND [Quantity] > 0))");
                table.HasCheckConstraint(
                    "CK_Checkout_ReturnedQuantity",
                    "[ReturnedQuantity] >= 0 AND [ReturnedQuantity] <= [Quantity]");
            });
            entity.HasKey(item => item.Id);
            entity.Property(item => item.Holder).HasMaxLength(200).IsRequired();
            entity.Property(item => item.ExpectedReturnAt).HasConversion(UtcTicksConverter);
            entity.Property(item => item.CheckedOutAt).HasConversion(UtcTicksConverter);
            entity.Property(item => item.ReturnedAt).HasConversion(UtcTicksConverter);
            entity.HasIndex(item => item.EquipmentAssetId)
                .IsUnique()
                .HasFilter("[EquipmentAssetId] IS NOT NULL AND [IsOpen] = 1");
            entity.HasOne<EquipmentAsset>()
                .WithMany()
                .HasForeignKey(item => item.EquipmentAssetId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<PooledStock>()
                .WithMany()
                .HasForeignKey(item => item.PooledStockId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<DroneApproval>()
                .WithMany()
                .HasForeignKey(item => item.ApprovalId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<DroneApproval>(entity =>
        {
            entity.HasKey(item => item.Id);
            entity.Property(item => item.Holder).HasMaxLength(200).IsRequired();
            entity.Property(item => item.ApprovedBy).HasMaxLength(100).IsRequired();
            entity.Property(item => item.ApprovedAt).HasConversion(UtcTicksConverter);
            entity.HasOne<EquipmentAsset>()
                .WithMany()
                .HasForeignKey(item => item.EquipmentAssetId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<InventoryMovement>(entity =>
        {
            entity.HasKey(item => item.Id);
            entity.Property(item => item.PerformedBy).HasMaxLength(200).IsRequired();
            entity.Property(item => item.Description).HasMaxLength(1000);
            entity.Property(item => item.Type).HasConversion<string>().HasMaxLength(16);
            entity.Property(item => item.OccurredAt).HasConversion(UtcTicksConverter);
            entity.HasIndex(item => item.OccurredAt);
            entity.HasOne<Checkout>()
                .WithMany()
                .HasForeignKey(item => item.CheckoutId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<EquipmentAsset>()
                .WithMany()
                .HasForeignKey(item => item.EquipmentAssetId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<PooledStock>()
                .WithMany()
                .HasForeignKey(item => item.PooledStockId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
