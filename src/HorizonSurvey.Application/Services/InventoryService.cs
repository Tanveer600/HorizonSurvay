using HorizonSurvey.Application.Contracts;
using HorizonSurvey.Domain.Contracts;
using HorizonSurvey.Domain.Errors;
using HorizonSurvey.Domain.Models;

namespace HorizonSurvey.Application.Services;

public sealed class InventoryService(IInventoryRepository repository, TimeProvider timeProvider)
{
    public Task<InventorySnapshot> GetInventoryAsync(CancellationToken cancellationToken = default) =>
        repository.GetInventoryAsync(cancellationToken);

    public Task<IReadOnlyList<OpenCheckout>> GetOpenCheckoutsAsync(CancellationToken cancellationToken = default) =>
        repository.GetOpenCheckoutsAsync(cancellationToken);

    public Task<IReadOnlyList<EquipmentCategory>> GetCategoriesAsync(CancellationToken cancellationToken = default) =>
        repository.GetCategoriesAsync(cancellationToken);

    public Task<EquipmentCategory> CreateCategoryAsync(CreateCategoryCommand command, CancellationToken cancellationToken = default)
    {
        var name = ValidateCategoryName(command.Name);
        return repository.CreateCategoryAsync(new EquipmentCategory(name), cancellationToken);
    }

    public Task<EquipmentCategory> UpdateCategoryAsync(UpdateCategoryCommand command, CancellationToken cancellationToken = default) =>
        repository.UpdateCategoryAsync(command.Id, ValidateCategoryName(command.Name), cancellationToken);

    public Task DeleteCategoryAsync(Guid id, CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty)
            throw new InventoryException("Category id is required.", InventoryErrorKind.InvalidInput);
        return repository.DeleteCategoryAsync(id, cancellationToken);
    }

    public Task<EquipmentAsset> RegisterEquipmentAsync(RegisterEquipmentCommand command,CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(command.Name) || command.Name.Trim().Length > 200)
            throw new InventoryException("Equipment name is required and cannot exceed 200 characters.", InventoryErrorKind.InvalidInput);
        if (command.CategoryId == Guid.Empty)
            throw new InventoryException("Equipment category is required.", InventoryErrorKind.InvalidInput);
        return repository.RegisterEquipmentAsync(
            new EquipmentAsset(command.Name.Trim(), command.CategoryId),
            cancellationToken);
    }

    public async Task DeleteMovementAsync(Guid id, CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty)
            throw new InventoryException("Movement id is required.", InventoryErrorKind.InvalidInput);
        await repository.DeleteMovementAsync(id, cancellationToken);
    }

    public Task<int> DeleteMovementsAsync(DateTimeOffset? from, DateTimeOffset? to, CancellationToken cancellationToken = default)
    {
        if (from is null || to is null)
            throw new InventoryException("Both from and to date-time values are required.", InventoryErrorKind.InvalidInput);
        if (from > to)
            throw new InventoryException("'from' must be earlier than or equal to 'to'.", InventoryErrorKind.InvalidInput);
        return repository.DeleteMovementsAsync(from.Value, to.Value, cancellationToken);
    }

    private static string ValidateCategoryName(string name)
    {
        var trimmed = name?.Trim();
        if (string.IsNullOrEmpty(trimmed) || trimmed.Length > 100)
            throw new InventoryException("Category name is required and cannot exceed 100 characters.", InventoryErrorKind.InvalidInput);
        return trimmed;
    }

    public Task<DroneApproval> RecordApprovalAsync(RecordApprovalCommand command,CancellationToken cancellationToken = default)
    {
        var approval = new DroneApproval(
            command.EquipmentAssetId,
            command.Holder,
            command.ApprovedBy,
            timeProvider.GetUtcNow());
        return repository.RecordApprovalAsync(approval, cancellationToken);
    }

    public Task<CheckoutOutcome> CheckoutAsync( CheckoutCommand command, CancellationToken cancellationToken = default)
    {
        var now = timeProvider.GetUtcNow();
        var checkout = new Checkout(
            command.Holder,
            command.ExpectedReturnAt,
            command.EquipmentAssetId,
            command.PooledStockId,
            command.Quantity,
            now,
            command.ApprovalId);
        return repository.CheckoutAsync(checkout, now, cancellationToken);
    }

    public Task<Checkout> ReturnAsync( ReturnCommand command,CancellationToken cancellationToken = default)
    {
        if (command.CheckoutId == Guid.Empty)
            throw new InventoryException("Checkout id is required.", InventoryErrorKind.InvalidInput);
        if (string.IsNullOrWhiteSpace(command.Returner))
            throw new InventoryException("Returner is required.", InventoryErrorKind.InvalidInput);
        if (command.Quantity <= 0)
            throw new InventoryException("Return quantity must be greater than zero.", InventoryErrorKind.InvalidInput);
        if (command.DamageDescription?.Length > 1000)
            throw new InventoryException("Damage description cannot exceed 1000 characters.", InventoryErrorKind.InvalidInput);
        if (string.IsNullOrWhiteSpace(command.DamageDescription) && command.DamageQuantity is not null)
            throw new InventoryException("Damage quantity requires a damage description.", InventoryErrorKind.InvalidInput);
        if (!string.IsNullOrWhiteSpace(command.DamageDescription) &&
            (command.DamageQuantity is null || command.DamageQuantity <= 0 ||
             command.DamageQuantity > command.Quantity))
            throw new InventoryException("Provide a valid damage quantity not exceeding the return quantity.", InventoryErrorKind.InvalidInput);

        return repository.ReturnAsync(
            command.CheckoutId,
            command.Returner.Trim(),
            command.Quantity,
            string.IsNullOrWhiteSpace(command.DamageDescription) ? null : command.DamageDescription.Trim(),
            command.DamageQuantity,
            timeProvider.GetUtcNow(),
            cancellationToken);
    }

    public Task<IReadOnlyList<MovementRecord>> GetHistoryAsync( DateTimeOffset? from,DateTimeOffset? to, CancellationToken cancellationToken = default)
    {
        if (from is null || to is null)
            throw new InventoryException("Both from and to date-time values are required.", InventoryErrorKind.InvalidInput);
        if (from > to)
            throw new InventoryException("'from' must be earlier than or equal to 'to'.", InventoryErrorKind.InvalidInput);

        return repository.GetHistoryAsync(from.Value, to.Value, cancellationToken);
    }
}
