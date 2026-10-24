using System.Data;
using HorizonSurvey.Domain.Contracts;
using HorizonSurvey.Domain.Errors;
using HorizonSurvey.Domain.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace HorizonSurvey.Infrastructure.Persistence;

public sealed class EfInventoryRepository(InventoryDbContext dbContext, TimeProvider timeProvider) : IInventoryRepository
{
    public async Task<InventorySnapshot> GetInventoryAsync(CancellationToken cancellationToken)
    {
        await using var transaction = await dbContext.Database.BeginTransactionAsync(IsolationLevel.Serializable,
             cancellationToken);
        var equipment = await dbContext.EquipmentAssets
            .AsNoTracking()
            .Include(item => item.Category)
            .OrderBy(item => item.Name)
            .ToListAsync(cancellationToken);
        var stocks = await dbContext.PooledStocks
            .AsNoTracking()
            .OrderBy(item => item.Name)
            .ToListAsync(cancellationToken);
        var activeCheckouts = await dbContext.Checkouts
            .AsNoTracking()
            .Where(item => item.IsOpen)
            .ToListAsync(cancellationToken);
        var latestEquipmentDamage = await dbContext.EquipmentAssets.AsNoTracking()
            .Select(asset => new
            {
                asset.Id,
                Description = dbContext.InventoryMovements.Where(movement => movement.EquipmentAssetId == asset.Id &&
                        movement.Type == MovementType.Damage)
                    .OrderByDescending(movement => movement.OccurredAt)
                    .Select(movement => movement.Description)
                    .FirstOrDefault()
            })
            .ToDictionaryAsync(item => item.Id, item => item.Description, cancellationToken);
        var latestStockDamage = await dbContext.PooledStocks
            .AsNoTracking()
            .Select(stock => new
            {
                stock.Id,
                Description = dbContext.InventoryMovements
                    .Where(movement => movement.PooledStockId == stock.Id &&movement.Type == MovementType.Damage)
                    .OrderByDescending(movement => movement.OccurredAt).Select(movement => movement.Description)
                    .FirstOrDefault()
            })
            .ToDictionaryAsync(item => item.Id, item => item.Description, cancellationToken);

        var currentByEquipment = activeCheckouts
            .Where(item => item.EquipmentAssetId is not null)
            .ToDictionary(item => item.EquipmentAssetId!.Value);
        var now = timeProvider.GetUtcNow();
        var overduePooledQuantities = activeCheckouts
            .Where(item => item.PooledStockId is not null && item.IsOverdue(now))
            .GroupBy(item => item.PooledStockId!.Value)
            .ToDictionary(group => group.Key, group => group.Sum(item => item.OutstandingQuantity));

        var snapshot = new InventorySnapshot(
            equipment.Select(item =>
            {
                currentByEquipment.TryGetValue(item.Id, out var checkout);
                latestEquipmentDamage.TryGetValue(item.Id, out var damage);
                return new EquipmentInventoryItem(
                    item.Id,
                    item.Name,
                    item.CategoryId,
                    item.Category?.Name,
                    checkout is null,
                    checkout?.Holder,
                    checkout?.ExpectedReturnAt,
                    checkout?.IsOverdue(now) ?? false,
                    damage);
            }).ToArray(),
            stocks.Select(item =>
            {
                latestStockDamage.TryGetValue(item.Id, out var damage);
                overduePooledQuantities.TryGetValue(item.Id, out var overdueQuantity);
                return new PooledInventoryItem(
                    item.Id,
                    item.Name,
                    item.TotalQuantity,
                    item.AvailableQuantity,
                    overdueQuantity,
                    damage);
            }).ToArray());
        await transaction.CommitAsync(cancellationToken);
        return snapshot;
    }

    public async Task<IReadOnlyList<OpenCheckout>> GetOpenCheckoutsAsync(CancellationToken cancellationToken)
    {
        var checkouts = await dbContext.Checkouts
            .AsNoTracking()
            .Where(item => item.IsOpen)
            .OrderBy(item => item.ExpectedReturnAt)
            .ToListAsync(cancellationToken);
        if (checkouts.Count == 0)
            return [];

        var equipmentIds = checkouts
            .Where(item => item.EquipmentAssetId is not null)
            .Select(item => item.EquipmentAssetId!.Value)
            .Distinct()
            .ToArray();
        var stockIds = checkouts
            .Where(item => item.PooledStockId is not null)
            .Select(item => item.PooledStockId!.Value)
            .Distinct()
            .ToArray();
        var equipment = await dbContext.EquipmentAssets
            .AsNoTracking()
            .Include(item => item.Category)
            .Where(item => equipmentIds.Contains(item.Id))
            .ToDictionaryAsync(item => item.Id, cancellationToken);
        var stocks = await dbContext.PooledStocks
            .AsNoTracking()
            .Where(item => stockIds.Contains(item.Id))
            .ToDictionaryAsync(item => item.Id, cancellationToken);
        var now = timeProvider.GetUtcNow();

        return checkouts.Select(checkout =>
        {
            var asset = checkout.EquipmentAssetId is Guid assetId ? equipment[assetId] : null;
            var stock = checkout.PooledStockId is Guid stockId ? stocks[stockId] : null;
            return new OpenCheckout(
                checkout.Id,
                checkout.EquipmentAssetId,
                checkout.PooledStockId,
                asset?.Name ?? stock!.Name,
                asset?.Category?.Name,
                checkout.Holder,
                checkout.ExpectedReturnAt,
                checkout.OutstandingQuantity,
                checkout.IsOverdue(now));
        }).ToArray();
    }

    public async Task<IReadOnlyList<EquipmentCategory>> GetCategoriesAsync(CancellationToken cancellationToken) =>
        await dbContext.EquipmentCategories.AsNoTracking().OrderBy(item => item.Name).ToListAsync(cancellationToken);

    public async Task<EquipmentCategory> CreateCategoryAsync(EquipmentCategory category, CancellationToken cancellationToken)
    {
        if (await dbContext.EquipmentCategories.AnyAsync(
                item => item.Name.ToLower() == category.Name.ToLower(),
                cancellationToken))
            throw new InventoryException("A category with this name already exists.", InventoryErrorKind.Conflict);

        dbContext.EquipmentCategories.Add(category);
        await dbContext.SaveChangesAsync(cancellationToken);
        return category;
    }

    public async Task<EquipmentCategory> UpdateCategoryAsync(Guid id, string name, CancellationToken cancellationToken)
    {
        var category = await dbContext.EquipmentCategories.SingleOrDefaultAsync(item => item.Id == id, cancellationToken);
        if (category is null)
            throw new InventoryException("Equipment category was not found.", InventoryErrorKind.NotFound);
        if (await dbContext.EquipmentCategories.AnyAsync(
                item => item.Id != id && item.Name.ToLower() == name.ToLower(),
                cancellationToken))
            throw new InventoryException("A category with this name already exists.", InventoryErrorKind.Conflict);
        category.Rename(name);
        await dbContext.SaveChangesAsync(cancellationToken);
        return category;
    }

    public async Task DeleteCategoryAsync(Guid id, CancellationToken cancellationToken)
    {
        await using var transaction = await dbContext.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
        var category = await dbContext.EquipmentCategories.SingleOrDefaultAsync(item => item.Id == id, cancellationToken);
        if (category is null)
            throw new InventoryException("Equipment category was not found.", InventoryErrorKind.NotFound);

        var assets = await dbContext.EquipmentAssets.Where(item => item.CategoryId == id).ToListAsync(cancellationToken);
        var assetIds = assets.Select(item => item.Id).ToArray();
        if (await dbContext.Checkouts.AnyAsync(
                item => item.IsOpen && item.EquipmentAssetId != null && assetIds.Contains(item.EquipmentAssetId.Value),
                cancellationToken))
            throw new InventoryException("This category has equipment with an open checkout and cannot be deleted.", InventoryErrorKind.Conflict);

        foreach (var asset in assets)
            dbContext.Entry(asset).Property(item => item.CategoryId).CurrentValue = null;
        dbContext.EquipmentCategories.Remove(category);
        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }

    public async Task<EquipmentAsset> RegisterEquipmentAsync(
        EquipmentAsset equipment,
        CancellationToken cancellationToken)
    {
        if (!await dbContext.EquipmentCategories.AnyAsync(item => item.Id == equipment.CategoryId, cancellationToken))
            throw new InventoryException("Equipment category was not found.", InventoryErrorKind.InvalidInput);
        dbContext.EquipmentAssets.Add(equipment);
        await dbContext.SaveChangesAsync(cancellationToken);
        return equipment;
    }

    public async Task<DroneApproval> RecordApprovalAsync(DroneApproval approval,CancellationToken cancellationToken)
    {
        var equipment = await dbContext.EquipmentAssets
            .Include(item => item.Category)
            .SingleOrDefaultAsync(item => item.Id == approval.EquipmentAssetId, cancellationToken);
        if (equipment is null)
            throw new InventoryException("Equipment asset was not found.", InventoryErrorKind.NotFound);
        if (equipment.Category?.RequiresApproval != true)
            throw new InventoryException("Approval records are only supported for drone checkouts.", InventoryErrorKind.InvalidInput);

        dbContext.DroneApprovals.Add(approval);
        await dbContext.SaveChangesAsync(cancellationToken);
        return approval;
    }

    public async Task<CheckoutOutcome> CheckoutAsync(Checkout checkout,DateTimeOffset occurredAt,CancellationToken cancellationToken)
    {
        try
        {
            await using var transaction = await dbContext.Database.BeginTransactionAsync(
                IsolationLevel.Serializable,
                cancellationToken);
            bool? assetAvailable = null;
            int? stockAvailableQuantity = null;

            if (checkout.EquipmentAssetId is Guid equipmentId)
            {
                var equipment = await dbContext.EquipmentAssets
                    .Include(item => item.Category)
                    .SingleOrDefaultAsync(item => item.Id == equipmentId, cancellationToken);
                if (equipment is null)
                    throw new InventoryException("Equipment asset was not found.", InventoryErrorKind.InvalidInput);

                var alreadyCheckedOut = await dbContext.Checkouts.AnyAsync(
                    item => item.EquipmentAssetId == equipmentId && item.IsOpen,
                    cancellationToken);
                if (alreadyCheckedOut)
                    throw new InventoryException("Equipment asset is already checked out.", InventoryErrorKind.Conflict);
                assetAvailable = false;

                if (equipment.Category?.RequiresApproval == true)
                {
                    if (checkout.ApprovalId is not Guid approvalId)
                        throw new InventoryException("A recorded Hamza approval is required for drone checkout.", InventoryErrorKind.Conflict);

                    var approval = await dbContext.DroneApprovals
                        .SingleOrDefaultAsync(item => item.Id == approvalId, cancellationToken);
                    if (approval is null ||
                        approval.EquipmentAssetId != equipmentId ||
                        !string.Equals(approval.Holder, checkout.Holder, StringComparison.OrdinalIgnoreCase) ||
                        !string.Equals(approval.ApprovedBy, "Hamza", StringComparison.OrdinalIgnoreCase))
                        throw new InventoryException("A matching recorded Hamza approval was not found.", InventoryErrorKind.Conflict);

                    approval.Consume(checkout.Id);
                }
                else if (checkout.ApprovalId is not null)
                {
                    throw new InventoryException("Drone approval cannot be applied to other equipment.", InventoryErrorKind.InvalidInput);
                }
            }
            else if (checkout.PooledStockId is Guid stockId)
            {
                if (checkout.ApprovalId is not null)
                    throw new InventoryException("Drone approval cannot be applied to pooled stock.", InventoryErrorKind.InvalidInput);

                var stock = await dbContext.PooledStocks
                    .SingleOrDefaultAsync(item => item.Id == stockId, cancellationToken);
                if (stock is null)
                    throw new InventoryException("Pooled stock item was not found.", InventoryErrorKind.InvalidInput);
                stock.Checkout(checkout.Quantity);
                stockAvailableQuantity = stock.AvailableQuantity;
            }

            dbContext.Checkouts.Add(checkout);
            dbContext.InventoryMovements.Add(new InventoryMovement(
                checkout.Id,
                checkout.EquipmentAssetId,
                checkout.PooledStockId,
                MovementType.Checkout,
                checkout.Quantity,
                occurredAt,
                checkout.Holder));
            await dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return new CheckoutOutcome(checkout, assetAvailable, stockAvailableQuantity);
        }
        catch (Exception exception) when (IsSqlServerConcurrencyConflict(exception))
        {
            throw new InventoryException(
                "Another inventory operation conflicted. Refresh and retry.",
                InventoryErrorKind.Conflict);
        }
    }

    public async Task<Checkout> ReturnAsync(Guid checkoutId,string returner,int quantity,string? damageDescription,int?damageQuantity,DateTimeOffset occurredAt,CancellationToken cancellationToken)
    {
        try
        {
            await using var transaction = await dbContext.Database.BeginTransactionAsync(
                IsolationLevel.Serializable,
                cancellationToken);

            var checkout = await dbContext.Checkouts
                .SingleOrDefaultAsync(item => item.Id == checkoutId, cancellationToken);
            if (checkout is null)
                throw new InventoryException("Checkout was not found.", InventoryErrorKind.NotFound);

            checkout.RecordReturn(returner, quantity, occurredAt);
            if (checkout.PooledStockId is Guid stockId)
            {
                var stock = await dbContext.PooledStocks
                    .SingleAsync(item => item.Id == stockId, cancellationToken);
                stock.Return(quantity);
            }

            dbContext.InventoryMovements.Add(new InventoryMovement(
                checkout.Id,
                checkout.EquipmentAssetId,
                checkout.PooledStockId,
                MovementType.Return,
                quantity,
                occurredAt,
                returner,
                damageDescription));
            if (damageDescription is not null)
            {
                dbContext.InventoryMovements.Add(new InventoryMovement(
                    checkout.Id,
                    checkout.EquipmentAssetId,
                    checkout.PooledStockId,
                    MovementType.Damage,
                    damageQuantity!.Value,
                    occurredAt,
                    returner,
                    damageDescription));
            }

            await dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return checkout;
        }
        catch (Exception exception) when (IsSqlServerConcurrencyConflict(exception))
        {
            throw new InventoryException(
                "Another inventory operation conflicted. Refresh and retry.",
                InventoryErrorKind.Conflict);
        }
    }

    public async Task<IReadOnlyList<MovementRecord>> GetHistoryAsync(
        DateTimeOffset from,
        DateTimeOffset to,
        CancellationToken cancellationToken)
    {
        var movements = await dbContext.InventoryMovements
            .AsNoTracking()
            .Where(item => item.OccurredAt >= from && item.OccurredAt <= to)
            .OrderBy(item => item.OccurredAt)
            .ToListAsync(cancellationToken);
        var equipmentIds = movements
            .Where(item => item.EquipmentAssetId is not null)
            .Select(item => item.EquipmentAssetId!.Value)
            .Distinct()
            .ToArray();
        var stockIds = movements
            .Where(item => item.PooledStockId is not null)
            .Select(item => item.PooledStockId!.Value)
            .Distinct()
            .ToArray();
        var equipmentNames = await dbContext.EquipmentAssets
            .AsNoTracking()
            .Where(item => equipmentIds.Contains(item.Id))
            .ToDictionaryAsync(item => item.Id, item => item.Name, cancellationToken);
        var stockNames = await dbContext.PooledStocks
            .AsNoTracking()
            .Where(item => stockIds.Contains(item.Id))
            .ToDictionaryAsync(item => item.Id, item => item.Name, cancellationToken);

        return movements.Select(item => new MovementRecord(
            item.Id,
            item.CheckoutId,
            item.EquipmentAssetId is Guid equipmentId
                ? equipmentNames[equipmentId]
                : stockNames[item.PooledStockId!.Value],
            item.Type,
            item.Quantity,
            item.OccurredAt,
            item.PerformedBy,
            item.Description)).ToArray();
    }

    public async Task DeleteMovementAsync(Guid id, CancellationToken cancellationToken)
    {
        var movement = await dbContext.InventoryMovements.SingleOrDefaultAsync(item => item.Id == id, cancellationToken);
        if (movement is null)
            throw new InventoryException("Movement record was not found.", InventoryErrorKind.NotFound);
        dbContext.InventoryMovements.Remove(movement);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<int> DeleteMovementsAsync(
        DateTimeOffset from,
        DateTimeOffset to,
        CancellationToken cancellationToken) =>
        await dbContext.InventoryMovements
            .Where(item => item.OccurredAt >= from && item.OccurredAt <= to)
            .ExecuteDeleteAsync(cancellationToken);

    private static bool IsSqlServerConcurrencyConflict(Exception exception)
    {
        for (var current = exception; current is not null; current = current.InnerException)
        {
            if (current is SqlException sqlException &&
                IsSqlServerConcurrencyErrorNumber(sqlException.Number))
                return true;
        }

        return false;
    }

    internal static bool IsSqlServerConcurrencyErrorNumber(int number) =>
        number is 1205 or 2601 or 2627;
}
