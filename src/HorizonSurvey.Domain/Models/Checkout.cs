using HorizonSurvey.Domain.Errors;

namespace HorizonSurvey.Domain.Models;

public sealed class Checkout
{
    private Checkout() { }

    public Checkout(
        string holder,
        DateTimeOffset expectedReturnAt,
        Guid? equipmentAssetId,
        Guid? pooledStockId,
        int quantity,
        DateTimeOffset checkedOutAt,
        Guid? approvalId)
    {
        if (string.IsNullOrWhiteSpace(holder))
            throw new InventoryException("Holder is required.", InventoryErrorKind.InvalidInput);
        if (expectedReturnAt <= checkedOutAt)
            throw new InventoryException("Expected return must be later than checkout time.", InventoryErrorKind.InvalidInput);
        if ((equipmentAssetId is null) == (pooledStockId is null))
            throw new InventoryException("Specify exactly one equipment asset or pooled stock item.", InventoryErrorKind.InvalidInput);
        if (quantity <= 0)
            throw new InventoryException("Quantity must be greater than zero.", InventoryErrorKind.InvalidInput);
        if (equipmentAssetId is not null && quantity != 1)
            throw new InventoryException("Individually tracked equipment must be checked out with quantity 1.", InventoryErrorKind.InvalidInput);

        Id = Guid.NewGuid();
        Holder = holder.Trim();
        ExpectedReturnAt = expectedReturnAt;
        EquipmentAssetId = equipmentAssetId;
        PooledStockId = pooledStockId;
        Quantity = quantity;
        CheckedOutAt = checkedOutAt;
        ApprovalId = approvalId;
    }

    public Guid Id { get; private set; }
    public string Holder { get; private set; } = string.Empty;
    public DateTimeOffset ExpectedReturnAt { get; private set; }
    public Guid? EquipmentAssetId { get; private set; }
    public Guid? PooledStockId { get; private set; }
    public int Quantity { get; private set; }
    public int ReturnedQuantity { get; private set; }
    public bool IsOpen { get; private set; } = true;
    public DateTimeOffset CheckedOutAt { get; private set; }
    public DateTimeOffset? ReturnedAt { get; private set; }
    public Guid? ApprovalId { get; private set; }
    public int OutstandingQuantity => Quantity - ReturnedQuantity;
    public bool IsOverdue(DateTimeOffset now) => IsOpen && now > ExpectedReturnAt;

    public void RecordReturn(string returner, int quantity, DateTimeOffset returnedAt)
    {
        if (string.IsNullOrWhiteSpace(returner))
            throw new InventoryException("Returner is required.", InventoryErrorKind.InvalidInput);
        if (quantity <= 0)
            throw new InventoryException("Return quantity must be greater than zero.", InventoryErrorKind.InvalidInput);
        if (quantity > OutstandingQuantity)
            throw new InventoryException("Return quantity exceeds the outstanding checkout quantity.", InventoryErrorKind.InvalidInput);

        ReturnedQuantity += quantity;
        if (ReturnedQuantity == Quantity)
        {
            ReturnedAt = returnedAt;
            IsOpen = false;
        }
    }
}
