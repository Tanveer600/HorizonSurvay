namespace HorizonSurvey.Domain.Models;

public sealed class InventoryMovement
{
    private InventoryMovement() { }

    public InventoryMovement(
        Guid checkoutId,
        Guid? equipmentAssetId,
        Guid? pooledStockId,
        MovementType type,
        int quantity,
        DateTimeOffset occurredAt,
        string performedBy,
        string? description = null)
    {
        Id = Guid.NewGuid();
        CheckoutId = checkoutId;
        EquipmentAssetId = equipmentAssetId;
        PooledStockId = pooledStockId;
        Type = type;
        Quantity = quantity;
        OccurredAt = occurredAt;
        PerformedBy = performedBy;
        Description = description;
    }

    public Guid Id { get; private set; }
    public Guid CheckoutId { get; private set; }
    public Guid? EquipmentAssetId { get; private set; }
    public Guid? PooledStockId { get; private set; }
    public MovementType Type { get; private set; }
    public int Quantity { get; private set; }
    public DateTimeOffset OccurredAt { get; private set; }
    public string PerformedBy { get; private set; } = string.Empty;
    public string? Description { get; private set; }
}
