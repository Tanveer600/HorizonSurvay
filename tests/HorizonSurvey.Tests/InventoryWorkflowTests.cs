using HorizonSurvey.Application.Contracts;
using HorizonSurvey.Application.Services;
using HorizonSurvey.Domain.Errors;
using HorizonSurvey.Domain.Models;
using HorizonSurvey.Infrastructure.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace HorizonSurvey.Tests;

public sealed class InventoryWorkflowTests
{
    [Fact]
    public async Task PooledCheckoutAndPartialReturnKeepCountsAndDamageHistoryConsistent()
    {
        await using var database = await TestDatabase.CreateAsync();
        var tripods = await database.AddPooledStockAsync("Tripods", 4);

        var outcome = await database.Service.CheckoutAsync(new CheckoutCommand(
            "Amina",
            DateTimeOffset.UtcNow.AddDays(1),
            null,
            tripods.Id,
            2,
            null));
        var checkout = outcome.Checkout;

        var afterCheckout = await database.Service.GetInventoryAsync();
        Assert.Equal(2, Assert.Single(afterCheckout.PooledStock, item => item.Id == tripods.Id).AvailableQuantity);

        await database.Service.ReturnAsync(new ReturnCommand(
            checkout.Id,
            "Saima",
            2,
            "One tripod has a damaged clamp.",
            1));

        var afterPartialReturn = await database.Service.GetInventoryAsync();
        Assert.Equal(4, Assert.Single(afterPartialReturn.PooledStock, item => item.Id == tripods.Id).AvailableQuantity);
        Assert.Contains("damaged clamp", Assert.Single(afterPartialReturn.PooledStock, item => item.Id == tripods.Id).LatestDamageDescription);

        var history = await database.Service.GetHistoryAsync(
            DateTimeOffset.UtcNow.AddMinutes(-1),
            DateTimeOffset.UtcNow.AddMinutes(1));
        Assert.Contains(history, item => item.Type == MovementType.Checkout && item.CheckoutId == checkout.Id);
        Assert.Contains(history, item => item.Type == MovementType.Return && item.CheckoutId == checkout.Id);
        Assert.Contains(history, item =>
            item.Type == MovementType.Damage &&
            item.Quantity == 1 &&
            item.Description == "One tripod has a damaged clamp.");
    }

    [Fact]
    public async Task InvalidCheckoutDoesNotPersistCheckoutOrMovement()
    {
        await using var database = await TestDatabase.CreateAsync();
        var tablets = await database.AddPooledStockAsync("Field tablets", 6);

        var unknown = await Assert.ThrowsAsync<InventoryException>(() =>
            database.Service.CheckoutAsync(new CheckoutCommand(
                "Amina",
                DateTimeOffset.UtcNow.AddDays(1),
                null,
                Guid.NewGuid(),
                1,
                null)));
        Assert.Equal(InventoryErrorKind.InvalidInput, unknown.Kind);

        var invalidQuantity = await Assert.ThrowsAsync<InventoryException>(() =>
            database.Service.CheckoutAsync(new CheckoutCommand(
                "Amina",
                DateTimeOffset.UtcNow.AddDays(1),
                null,
                tablets.Id,
                0,
                null)));
        Assert.Equal(InventoryErrorKind.InvalidInput, invalidQuantity.Kind);

        var unavailable = await Assert.ThrowsAsync<InventoryException>(() =>
            database.Service.CheckoutAsync(new CheckoutCommand(
                "Amina",
                DateTimeOffset.UtcNow.AddDays(1),
                null,
                tablets.Id,
                7,
                null)));
        Assert.Equal(InventoryErrorKind.Conflict, unavailable.Kind);

        var missingDueDate = await Assert.ThrowsAsync<InventoryException>(() =>
            database.Service.CheckoutAsync(new CheckoutCommand(
                "Amina",
                DateTimeOffset.MinValue,
                null,
                tablets.Id,
                1,
                null)));
        Assert.Equal(InventoryErrorKind.InvalidInput, missingDueDate.Kind);
        Assert.Equal(0, await database.Context.Checkouts.CountAsync());
        Assert.Equal(0, await database.Context.InventoryMovements.CountAsync());
        Assert.Equal(6, Assert.Single((await database.Service.GetInventoryAsync()).PooledStock, item => item.Id == tablets.Id).AvailableQuantity);
    }

    [Fact]
    public async Task DroneCheckoutRequiresMatchingRecordedHamzaApprovalAndConsumesItOnce()
    {
        await using var database = await TestDatabase.CreateAsync();
        var droneCategory = await database.AddCategoryAsync("Drone");
        var drone = await database.Service.RegisterEquipmentAsync(
            new RegisterEquipmentCommand("Drone 1", droneCategory.Id));
        var due = DateTimeOffset.UtcNow.AddDays(1);

        var roverCategory = await database.AddCategoryAsync("Rover");
        var rover = await database.Service.RegisterEquipmentAsync(
            new RegisterEquipmentCommand("Rover 1", roverCategory.Id));
        var nonDroneApproval = await Assert.ThrowsAsync<InventoryException>(() =>
            database.Service.RecordApprovalAsync(new RecordApprovalCommand(rover.Id, "Amina", "Hamza")));
        Assert.Equal(InventoryErrorKind.InvalidInput, nonDroneApproval.Kind);
        Assert.Equal(0, await database.Context.DroneApprovals.CountAsync());

        var unapproved = await Assert.ThrowsAsync<InventoryException>(() =>
            database.Service.CheckoutAsync(new CheckoutCommand("Hamza", due, drone.Id, null, 1, null)));
        Assert.Equal(InventoryErrorKind.Conflict, unapproved.Kind);
        Assert.Equal(0, await database.Context.Checkouts.CountAsync());
        Assert.Equal(0, await database.Context.InventoryMovements.CountAsync());

        await Assert.ThrowsAsync<InventoryException>(() => database.Service.RecordApprovalAsync(
            new RecordApprovalCommand(drone.Id, "Hamza", "Someone else")));

        var approval = await database.Service.RecordApprovalAsync(
            new RecordApprovalCommand(drone.Id, "Hamza", "hamza"));
        var outcome = await database.Service.CheckoutAsync(
            new CheckoutCommand("Hamza", due, drone.Id, null, 1, approval.Id));
        var checkout = outcome.Checkout;
        await database.Service.ReturnAsync(new ReturnCommand(checkout.Id, "Hamza", 1, null));

        var replay = await Assert.ThrowsAsync<InventoryException>(() =>
            database.Service.CheckoutAsync(new CheckoutCommand("Hamza", due, drone.Id, null, 1, approval.Id)));
        Assert.Equal(InventoryErrorKind.Conflict, replay.Kind);
        Assert.Equal(1, await database.Context.Checkouts.CountAsync());
        Assert.Equal(2, await database.Context.InventoryMovements.CountAsync());
    }

    [Fact]
    public async Task IndividualAssetCannotBeCheckedOutTwiceAndInvalidReturnsDoNotChangeState()
    {
        await using var database = await TestDatabase.CreateAsync();
        var category = await database.AddCategoryAsync("Rover");
        var asset = await database.Service.RegisterEquipmentAsync(
            new RegisterEquipmentCommand("Rover 1", category.Id));
        var outcome = await database.Service.CheckoutAsync(
            new CheckoutCommand("Amina", DateTimeOffset.UtcNow.AddDays(1), asset.Id, null, 1, null));
        var checkout = outcome.Checkout;

        var duplicate = await Assert.ThrowsAsync<InventoryException>(() =>
            database.Service.CheckoutAsync(new CheckoutCommand(
                "Saima",
                DateTimeOffset.UtcNow.AddDays(1),
                asset.Id,
                null,
                1,
                null)));
        Assert.Equal(InventoryErrorKind.Conflict, duplicate.Kind);

        var excessiveReturn = await Assert.ThrowsAsync<InventoryException>(() =>
            database.Service.ReturnAsync(new ReturnCommand(checkout.Id, "Saima", 2, null)));
        Assert.Equal(InventoryErrorKind.InvalidInput, excessiveReturn.Kind);

        var excessiveDamageQuantity = await Assert.ThrowsAsync<InventoryException>(() =>
            database.Service.ReturnAsync(new ReturnCommand(
                checkout.Id,
                "Saima",
                1,
                "One item is damaged.",
                2)));
        Assert.Equal(InventoryErrorKind.InvalidInput, excessiveDamageQuantity.Kind);

        var unmatchedReturn = await Assert.ThrowsAsync<InventoryException>(() =>
            database.Service.ReturnAsync(new ReturnCommand(Guid.NewGuid(), "Saima", 1, null)));
        Assert.Equal(InventoryErrorKind.NotFound, unmatchedReturn.Kind);
        Assert.Equal(1, await database.Context.Checkouts.CountAsync());
        Assert.Equal(1, await database.Context.InventoryMovements.CountAsync());
        Assert.False(Assert.Single((await database.Service.GetInventoryAsync()).Equipment).IsAvailable);
    }

    [Fact]
    public async Task HistoryRequiresBothOrderedBoundsAndFiltersMovementsToTheRequestedRange()
    {
        await using var database = await TestDatabase.CreateAsync();
        var stock = await database.AddPooledStockAsync("Tripods", 4);
        await database.Service.CheckoutAsync(new CheckoutCommand(
            "Amina",
            database.Clock.GetUtcNow().AddSeconds(1),
            null,
            stock.Id,
            1,
            null));
        var movementAt = database.Clock.GetUtcNow();
        var exactBoundary = await database.Service.GetHistoryAsync(movementAt, movementAt);
        Assert.Single(exactBoundary);
        database.Clock.Advance(TimeSpan.FromSeconds(1));
        var atDueInstant = await database.Service.GetInventoryAsync();
        Assert.Equal(0, Assert.Single(atDueInstant.PooledStock, item => item.Id == stock.Id).OverdueQuantity);
        database.Clock.Advance(TimeSpan.FromTicks(1));
        var afterDueInstant = await database.Service.GetInventoryAsync();
        Assert.Equal(1, Assert.Single(afterDueInstant.PooledStock, item => item.Id == stock.Id).OverdueQuantity);
        var overdueOpenCheckout = Assert.Single(await database.Service.GetOpenCheckoutsAsync());
        Assert.True(overdueOpenCheckout.IsOverdue);
        Assert.Equal(1, overdueOpenCheckout.OutstandingQuantity);

        var category = await database.AddCategoryAsync("Rover");
        var asset = await database.Service.RegisterEquipmentAsync(
            new RegisterEquipmentCommand("Overdue rover", category.Id));
        await database.Service.CheckoutAsync(new CheckoutCommand(
            "Amina",
            database.Clock.GetUtcNow().AddSeconds(1),
            asset.Id,
            null,
            1,
            null));
        Assert.False(Assert.Single(
            (await database.Service.GetInventoryAsync()).Equipment,
            item => item.Id == asset.Id).IsOverdue);
        database.Clock.Advance(TimeSpan.FromSeconds(1));
        Assert.False(Assert.Single(
            (await database.Service.GetInventoryAsync()).Equipment,
            item => item.Id == asset.Id).IsOverdue);
        database.Clock.Advance(TimeSpan.FromTicks(1));
        Assert.True(Assert.Single(
            (await database.Service.GetInventoryAsync()).Equipment,
            item => item.Id == asset.Id).IsOverdue);

        var unbounded = await Assert.ThrowsAsync<InventoryException>(() =>
            database.Service.GetHistoryAsync(null, DateTimeOffset.UtcNow));
        Assert.Equal(InventoryErrorKind.InvalidInput, unbounded.Kind);

        var reversed = await Assert.ThrowsAsync<InventoryException>(() =>
            database.Service.GetHistoryAsync(DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddDays(-1)));
        Assert.Equal(InventoryErrorKind.InvalidInput, reversed.Kind);

        var inRange = await database.Service.GetHistoryAsync(movementAt, movementAt);
        Assert.Single(inRange);
        var beforeCheckout = await database.Service.GetHistoryAsync(
            movementAt.AddSeconds(-1),
            movementAt.AddTicks(-1));
        Assert.Empty(beforeCheckout);
    }

    [Theory]
    [InlineData(1205, true)]
    [InlineData(2601, true)]
    [InlineData(2627, true)]
    [InlineData(547, false)]
    public void SqlServerConcurrencyErrorNumbersAreRecognized(int number, bool expected) =>
        Assert.Equal(expected, EfInventoryRepository.IsSqlServerConcurrencyErrorNumber(number));

    [Fact]
    public async Task ConcurrentCheckoutsCannotIssueAnIndividualAssetTwice()
    {
        await using var database = await TestDatabase.CreateAsync();
        var category = await database.AddCategoryAsync("Rover");
        var asset = await database.Service.RegisterEquipmentAsync(
            new RegisterEquipmentCommand("Concurrent rover", category.Id));
        await using var firstSession = await database.CreateServiceSessionAsync();
        await using var secondSession = await database.CreateServiceSessionAsync();
        var due = DateTimeOffset.UtcNow.AddDays(1);

        async Task<bool> AttemptCheckout(InventoryService service)
        {
            try
            {
                await service.CheckoutAsync(new CheckoutCommand(
                    "Amina",
                    due,
                    asset.Id,
                    null,
                    1,
                    null));
                return true;
            }
            catch (InventoryException exception)
            {
                Assert.Equal(InventoryErrorKind.Conflict, exception.Kind);
                return false;
            }
        }

        var results = await Task.WhenAll(
            AttemptCheckout(firstSession.Service),
            AttemptCheckout(secondSession.Service));

        Assert.Single(results, succeeded => succeeded);
        Assert.Equal(1, await database.Context.Checkouts.CountAsync());
        Assert.Equal(1, await database.Context.InventoryMovements.CountAsync());
    }

    [Fact]
    public async Task ConcurrentPooledCheckoutsCannotIssueMoreThanAvailableStock()
    {
        await using var database = await TestDatabase.CreateAsync();
        var stock = await database.AddPooledStockAsync("Tripods", 4);
        await using var firstSession = await database.CreateServiceSessionAsync();
        await using var secondSession = await database.CreateServiceSessionAsync();
        var due = DateTimeOffset.UtcNow.AddDays(1);

        async Task<bool> AttemptCheckout(InventoryService service)
        {
            try
            {
                await service.CheckoutAsync(new CheckoutCommand(
                    "Amina",
                    due,
                    null,
                    stock.Id,
                    3,
                    null));
                return true;
            }
            catch (InventoryException exception)
            {
                Assert.Equal(InventoryErrorKind.Conflict, exception.Kind);
                return false;
            }
        }

        var results = await Task.WhenAll(
            AttemptCheckout(firstSession.Service),
            AttemptCheckout(secondSession.Service));

        Assert.Single(results, succeeded => succeeded);
        Assert.Equal(1, await database.Context.Checkouts.CountAsync());
        Assert.Equal(1, await database.Context.InventoryMovements.CountAsync());
        Assert.Equal(1, Assert.Single(
            (await database.Service.GetInventoryAsync()).PooledStock,
            item => item.Id == stock.Id).AvailableQuantity);
    }

    [Fact]
    public async Task CategoryCrudValidatesUniqueNamesAndUpdatesExistingEquipmentMeaning()
    {
        await using var database = await TestDatabase.CreateAsync();
        var category = await database.Service.CreateCategoryAsync(new CreateCategoryCommand(" Drone "));
        var asset = await database.Service.RegisterEquipmentAsync(new RegisterEquipmentCommand("Drone 1", category.Id));

        var duplicate = await Assert.ThrowsAsync<InventoryException>(() =>
            database.Service.CreateCategoryAsync(new CreateCategoryCommand("drone")));
        Assert.Equal(InventoryErrorKind.Conflict, duplicate.Kind);
        var renamed = await database.Service.UpdateCategoryAsync(new UpdateCategoryCommand(category.Id, "Survey drone"));
        Assert.Equal("Survey drone", renamed.Name);
        Assert.Equal("Survey drone", Assert.Single((await database.Service.GetInventoryAsync()).Equipment).Category);
        Assert.Equal(asset.Id, Assert.Single((await database.Service.GetInventoryAsync()).Equipment).Id);
    }

    [Fact]
    public async Task DroneApprovalIsCaseInsensitiveAndDependsOnTheCurrentCategoryName()
    {
        await using var database = await TestDatabase.CreateAsync();
        var category = await database.AddCategoryAsync("dRoNe");
        var asset = await database.Service.RegisterEquipmentAsync(new RegisterEquipmentCommand("Drone 1", category.Id));
        var due = DateTimeOffset.UtcNow.AddDays(1);

        var missingApproval = await Assert.ThrowsAsync<InventoryException>(() =>
            database.Service.CheckoutAsync(new CheckoutCommand("Amina", due, asset.Id, null, 1, null)));
        Assert.Equal(InventoryErrorKind.Conflict, missingApproval.Kind);

        await database.Service.UpdateCategoryAsync(new UpdateCategoryCommand(category.Id, "Aerial equipment"));
        var checkout = await database.Service.CheckoutAsync(
            new CheckoutCommand("Amina", due, asset.Id, null, 1, null));
        await database.Service.ReturnAsync(new ReturnCommand(checkout.Checkout.Id, "Saima", 1, null));

        await database.Service.UpdateCategoryAsync(new UpdateCategoryCommand(category.Id, "DRONE"));
        var renamedBackToDrone = await Assert.ThrowsAsync<InventoryException>(() =>
            database.Service.CheckoutAsync(new CheckoutCommand("Amina", due, asset.Id, null, 1, null)));
        Assert.Equal(InventoryErrorKind.Conflict, renamedBackToDrone.Kind);
    }

    [Fact]
    public async Task CategoryDeletionRejectsOpenCheckoutAndOtherwisePreservesAssetsAndMovementHistory()
    {
        await using var database = await TestDatabase.CreateAsync();
        var category = await database.AddCategoryAsync("Rover");
        var asset = await database.Service.RegisterEquipmentAsync(new RegisterEquipmentCommand("Rover 1", category.Id));
        var checkout = await database.Service.CheckoutAsync(new CheckoutCommand(
            "Amina", DateTimeOffset.UtcNow.AddDays(1), asset.Id, null, 1, null));

        var conflict = await Assert.ThrowsAsync<InventoryException>(() => database.Service.DeleteCategoryAsync(category.Id));
        Assert.Equal(InventoryErrorKind.Conflict, conflict.Kind);
        Assert.Equal(1, await database.Context.EquipmentCategories.CountAsync());

        await database.Service.ReturnAsync(new ReturnCommand(checkout.Checkout.Id, "Saima", 1, null));
        await database.Service.DeleteCategoryAsync(category.Id);

        Assert.Empty(await database.Context.EquipmentCategories.ToListAsync());
        Assert.Equal(1, await database.Context.EquipmentAssets.CountAsync());
        Assert.Equal(1, await database.Context.Checkouts.CountAsync());
        Assert.Equal(2, await database.Context.InventoryMovements.CountAsync());
        var item = Assert.Single((await database.Service.GetInventoryAsync()).Equipment);
        Assert.Null(item.CategoryId);
        Assert.Null(item.Category);
    }

    [Fact]
    public async Task MovementDeletionRemovesOnlySelectedRowsAndBulkRangeIsInclusive()
    {
        await using var database = await TestDatabase.CreateAsync();
        var stock = await database.AddPooledStockAsync("Tripods", 4);
        var checkout = await database.Service.CheckoutAsync(new CheckoutCommand(
            "Amina", DateTimeOffset.UtcNow.AddDays(1), null, stock.Id, 1, null));
        var initialMovement = Assert.Single(await database.Service.GetHistoryAsync(
            DateTimeOffset.UtcNow.AddMinutes(-1), DateTimeOffset.UtcNow.AddMinutes(1)));
        await database.Service.ReturnAsync(new ReturnCommand(checkout.Checkout.Id, "Saima", 1, null));
        var returnMovement = (await database.Service.GetHistoryAsync(
            DateTimeOffset.UtcNow.AddMinutes(-1), DateTimeOffset.UtcNow.AddMinutes(1)))
            .Single(item => item.Type == MovementType.Return);

        await database.Service.DeleteMovementAsync(initialMovement.Id);
        Assert.Equal(1, await database.Context.InventoryMovements.CountAsync());
        Assert.Equal(1, await database.Context.Checkouts.CountAsync());
        Assert.Equal(4, (await database.Context.PooledStocks.SingleAsync()).AvailableQuantity);

        database.Clock.Advance(TimeSpan.FromDays(2));
        await database.Service.CheckoutAsync(new CheckoutCommand(
            "Amina", database.Clock.GetUtcNow().AddDays(1), null, stock.Id, 1, null));
        var deletedCount = await database.Service.DeleteMovementsAsync(returnMovement.OccurredAt, returnMovement.OccurredAt);

        Assert.Equal(1, deletedCount);
        Assert.Equal(1, await database.Context.InventoryMovements.CountAsync());
        Assert.Equal(2, await database.Context.Checkouts.CountAsync());
        Assert.Equal(3, (await database.Context.PooledStocks.SingleAsync()).AvailableQuantity);
    }

    private sealed class TestDatabase : IAsyncDisposable
    {
        private readonly string _connectionString;
        private readonly SqliteConnection _connection;

        private TestDatabase(
            string connectionString,
            SqliteConnection connection,
            InventoryDbContext context,
            TestTimeProvider clock)
        {
            _connectionString = connectionString;
            _connection = connection;
            Context = context;
            Clock = clock;
            Service = new InventoryService(new EfInventoryRepository(context, clock), clock);
        }

        public InventoryDbContext Context { get; }
        public InventoryService Service { get; }
        public TestTimeProvider Clock { get; }

        public async Task<PooledStock> AddPooledStockAsync(string name, int quantity)
        {
            var stock = new PooledStock(name, quantity);
            Context.PooledStocks.Add(stock);
            await Context.SaveChangesAsync();
            return stock;
        }

        public async Task<EquipmentCategory> AddCategoryAsync(string name)
        {
            var category = new EquipmentCategory(name);
            Context.EquipmentCategories.Add(category);
            await Context.SaveChangesAsync();
            return category;
        }

        public static async Task<TestDatabase> CreateAsync()
        {
            var connectionString = $"Data Source=HorizonSurveyTests-{Guid.NewGuid()};Mode=Memory;Cache=Shared";
            var connection = new SqliteConnection(connectionString);
            await connection.OpenAsync();
            var options = new DbContextOptionsBuilder<InventoryDbContext>()
                .UseSqlite(connection)
                .Options;
            var context = new InventoryDbContext(options);
            await context.Database.EnsureCreatedAsync();
            var clock = new TestTimeProvider(DateTimeOffset.UtcNow);
            return new TestDatabase(connectionString, connection, context, clock);
        }

        public async Task<ServiceSession> CreateServiceSessionAsync()
        {
            var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();
            var options = new DbContextOptionsBuilder<InventoryDbContext>()
                .UseSqlite(connection)
                .Options;
            var context = new InventoryDbContext(options);
            return new ServiceSession(connection, context, Clock);
        }

        public async ValueTask DisposeAsync()
        {
            await Context.DisposeAsync();
            await _connection.DisposeAsync();
        }
    }

    private sealed class ServiceSession(
        SqliteConnection connection,
        InventoryDbContext context,
        TestTimeProvider clock) : IAsyncDisposable
    {
        public InventoryService Service { get; } =
            new(new EfInventoryRepository(context, clock), clock);

        public async ValueTask DisposeAsync()
        {
            await context.DisposeAsync();
            await connection.DisposeAsync();
        }
    }

    private sealed class TestTimeProvider(DateTimeOffset now) : TimeProvider
    {
        private DateTimeOffset _now = now;

        public override DateTimeOffset GetUtcNow() => _now;

        public void Advance(TimeSpan amount) => _now = _now.Add(amount);
    }
}
