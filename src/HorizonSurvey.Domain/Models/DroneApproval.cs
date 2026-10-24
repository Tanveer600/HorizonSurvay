using HorizonSurvey.Domain.Errors;

namespace HorizonSurvey.Domain.Models;

public sealed class DroneApproval
{
    private DroneApproval() { }

    public DroneApproval(Guid equipmentAssetId, string holder, string approvedBy, DateTimeOffset approvedAt)
    {
        if (equipmentAssetId == Guid.Empty)
            throw new InventoryException("Equipment asset is required for drone approval.", InventoryErrorKind.InvalidInput);
        if (string.IsNullOrWhiteSpace(holder))
            throw new InventoryException("Approved holder is required.", InventoryErrorKind.InvalidInput);
        if (!string.Equals(approvedBy?.Trim(), "Hamza", StringComparison.OrdinalIgnoreCase))
            throw new InventoryException("Drone checkout approval must be recorded by Hamza.", InventoryErrorKind.InvalidInput);

        Id = Guid.NewGuid();
        EquipmentAssetId = equipmentAssetId;
        Holder = holder.Trim();
        ApprovedBy = "Hamza";
        ApprovedAt = approvedAt;
    }

    public Guid Id { get; private set; }
    public Guid EquipmentAssetId { get; private set; }
    public string Holder { get; private set; } = string.Empty;
    public string ApprovedBy { get; private set; } = string.Empty;
    public DateTimeOffset ApprovedAt { get; private set; }
    public Guid? CheckoutId { get; private set; }

    public void Consume(Guid checkoutId)
    {
        if (CheckoutId is not null)
            throw new InventoryException("Drone approval has already been used.", InventoryErrorKind.Conflict);
        CheckoutId = checkoutId;
    }
}
