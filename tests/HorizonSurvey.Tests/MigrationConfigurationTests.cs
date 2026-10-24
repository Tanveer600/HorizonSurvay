using HorizonSurvey.Application.Services;
using HorizonSurvey.Domain.Models;
using HorizonSurvey.Infrastructure;
using HorizonSurvey.Infrastructure.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HorizonSurvey.Tests;

public sealed class MigrationConfigurationTests
{
    [Fact]
    public async Task MigrationsCreateSchemaHistoryWithoutDefaultCategoriesOrPooledStock()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        var options = new DbContextOptionsBuilder<InventoryDbContext>()
            .UseSqlite(connection, sqlite =>
                sqlite.MigrationsAssembly(typeof(InventoryDbContext).Assembly.FullName))
            .Options;
        await using var dbContext = new InventoryDbContext(options);

        await dbContext.Database.MigrateAsync();

        Assert.Contains(await dbContext.Database.GetAppliedMigrationsAsync(),
            migration => migration.EndsWith("_InitialCreate", StringComparison.Ordinal));
        Assert.Contains(await dbContext.Database.GetAppliedMigrationsAsync(),
            migration => migration.EndsWith("_ManageEquipmentCategories", StringComparison.Ordinal));
        Assert.Empty(await dbContext.PooledStocks.AsNoTracking().ToListAsync());
        Assert.Empty(await dbContext.EquipmentCategories.AsNoTracking().ToListAsync());

        var inventory = await new InventoryService(
            new EfInventoryRepository(dbContext, TimeProvider.System),
            TimeProvider.System).GetInventoryAsync();
        Assert.Empty(inventory.PooledStock);
    }

    [Fact]
    public async Task InitialMigrationRejectsMultipleOpenCheckoutsForOneAsset()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        var options = new DbContextOptionsBuilder<InventoryDbContext>()
            .UseSqlite(connection, sqlite =>
                sqlite.MigrationsAssembly(typeof(InventoryDbContext).Assembly.FullName))
            .Options;
        await using var dbContext = new InventoryDbContext(options);
        await dbContext.Database.MigrateAsync();

        var category = new EquipmentCategory("Rover");
        var asset = new EquipmentAsset("Rover", category.Id);
        var now = DateTimeOffset.UtcNow;
        dbContext.EquipmentAssets.Add(asset);
        dbContext.EquipmentCategories.Add(category);
        dbContext.Checkouts.Add(new Checkout(
            "First holder",
            now.AddHours(1),
            asset.Id,
            null,
            1,
            now,
            null));
        await dbContext.SaveChangesAsync();

        dbContext.Checkouts.Add(new Checkout(
            "Second holder",
            now.AddHours(1),
            asset.Id,
            null,
            1,
            now,
            null));

        await Assert.ThrowsAsync<DbUpdateException>(() => dbContext.SaveChangesAsync());
    }

    [Fact]
    public async Task InitialMigrationAllowsRecheckoutAfterAnAssetIsReturned()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        var options = new DbContextOptionsBuilder<InventoryDbContext>()
            .UseSqlite(connection, sqlite =>
                sqlite.MigrationsAssembly(typeof(InventoryDbContext).Assembly.FullName))
            .Options;
        await using var dbContext = new InventoryDbContext(options);
        await dbContext.Database.MigrateAsync();

        var category = new EquipmentCategory("Rover");
        var asset = new EquipmentAsset("Rover", category.Id);
        var checkedOutAt = DateTimeOffset.UtcNow;
        var firstCheckout = new Checkout(
            "First holder",
            checkedOutAt.AddHours(1),
            asset.Id,
            null,
            1,
            checkedOutAt,
            null);
        dbContext.EquipmentAssets.Add(asset);
        dbContext.EquipmentCategories.Add(category);
        dbContext.Checkouts.Add(firstCheckout);
        await dbContext.SaveChangesAsync();

        firstCheckout.RecordReturn("First holder", 1, checkedOutAt.AddMinutes(30));
        await dbContext.SaveChangesAsync();

        dbContext.Checkouts.Add(new Checkout(
            "Second holder",
            checkedOutAt.AddHours(2),
            asset.Id,
            null,
            1,
            checkedOutAt.AddMinutes(30),
            null));
        await dbContext.SaveChangesAsync();

        Assert.Equal(2, await dbContext.Checkouts.CountAsync());
    }

    [Fact]
    public async Task CategoryMigrationPreservesExistingEquipmentCheckoutAndMovementData()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        var options = new DbContextOptionsBuilder<InventoryDbContext>()
            .UseSqlite(connection, sqlite => sqlite.MigrationsAssembly(typeof(InventoryDbContext).Assembly.FullName))
            .Options;
        await using var dbContext = new InventoryDbContext(options);
        var migrator = dbContext.GetService<IMigrator>();
        await migrator.MigrateAsync("20261021090940_InitialCreate");

        var assetId = Guid.NewGuid();
        var checkoutId = Guid.NewGuid();
        var movementId = Guid.NewGuid();
        var now = DateTimeOffset.UtcNow;
        var due = now.AddDays(1);
        await dbContext.Database.ExecuteSqlInterpolatedAsync(
            $"INSERT INTO EquipmentAssets (Id, Name, Category) VALUES ({assetId}, {"Rover 1"}, {"Rover"})");
        await dbContext.Database.ExecuteSqlInterpolatedAsync(
            $"INSERT INTO Checkouts (Id, Holder, ExpectedReturnAt, EquipmentAssetId, PooledStockId, Quantity, ReturnedQuantity, IsOpen, CheckedOutAt, ReturnedAt, ApprovalId) VALUES ({checkoutId}, {"Amina"}, {due.UtcDateTime.Ticks}, {assetId}, {null}, {1}, {0}, {true}, {now.UtcDateTime.Ticks}, {null}, {null})");
        await dbContext.Database.ExecuteSqlInterpolatedAsync(
            $"INSERT INTO InventoryMovements (Id, CheckoutId, EquipmentAssetId, PooledStockId, Type, Quantity, OccurredAt, PerformedBy, Description) VALUES ({movementId}, {checkoutId}, {assetId}, {null}, {"Checkout"}, {1}, {now.UtcDateTime.Ticks}, {"Amina"}, {null})");

        await migrator.MigrateAsync();

        var migratedAsset = await dbContext.EquipmentAssets.Include(item => item.Category).SingleAsync();
        Assert.Equal("Rover", migratedAsset.Category?.Name);
        Assert.Equal(assetId, migratedAsset.Id);
        Assert.Equal(checkoutId, (await dbContext.Checkouts.SingleAsync()).Id);
        Assert.Equal(movementId, (await dbContext.InventoryMovements.SingleAsync()).Id);
        Assert.Equal(new[] { "Rover" }, await dbContext.EquipmentCategories.Select(item => item.Name).ToArrayAsync());
    }

    [Fact]
    public void SqlServerContextDiscoversInfrastructureMigrationWithoutSeedStock()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:HorizonSurvey"] = "Server=(localdb)\\MSSQLLocalDB;Database=MigrationModelTest"
            })
            .Build();
        var services = new ServiceCollection();
        services.AddInventoryInfrastructure(configuration);

        using var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<InventoryDbContext>();

        Assert.Contains(dbContext.Database.GetMigrations(), migration =>
            migration.EndsWith("_InitialCreate", StringComparison.Ordinal));

        var migrationScript = dbContext.GetService<IMigrator>().GenerateScript();
        Assert.Contains("CREATE TABLE [EquipmentAssets]", migrationScript, StringComparison.Ordinal);
        Assert.Contains("CREATE UNIQUE INDEX [IX_Checkouts_EquipmentAssetId]", migrationScript, StringComparison.Ordinal);
        Assert.Contains("CREATE TABLE [__EFMigrationsHistory]", migrationScript, StringComparison.Ordinal);

        var migrationAssembly = dbContext.GetService<IMigrationsAssembly>();
        var migrationType = Assert.Single(
            migrationAssembly.Migrations.Where(pair => pair.Key.EndsWith("_InitialCreate", StringComparison.Ordinal)))
            .Value;
        var initialMigration = migrationAssembly.CreateMigration(
            migrationType,
            dbContext.Database.ProviderName!);
        Assert.DoesNotContain(
            initialMigration.UpOperations.OfType<InsertDataOperation>(),
            operation => operation.Table == "PooledStocks");
        Assert.Contains(dbContext.Database.GetMigrations(), migration =>
            migration.EndsWith("_ManageEquipmentCategories", StringComparison.Ordinal));
    }
}
