namespace HorizonSurvey.Domain.Models;

public sealed record EquipmentInventoryItem(Guid Id,string Name,Guid? CategoryId,string? Category,bool IsAvailable,string? CurrentHolder,DateTimeOffset? ExpectedReturnAt,bool IsOverdue,string? LatestDamageDescription);

public sealed record PooledInventoryItem( Guid Id, string Name,int TotalQuantity,int AvailableQuantity,int OverdueQuantity,string? LatestDamageDescription);

public sealed record InventorySnapshot(IReadOnlyList<EquipmentInventoryItem> Equipment,IReadOnlyList<PooledInventoryItem> PooledStock);

public sealed record CheckoutOutcome(Checkout Checkout,bool? AssetAvailable,int? StockAvailableQuantity);

public sealed record OpenCheckout(
    Guid Id,
    Guid? EquipmentAssetId,
    Guid? PooledStockId,
    string ItemName,
    string? Category,
    string Holder,
    DateTimeOffset ExpectedReturnAt,
    int OutstandingQuantity,
    bool IsOverdue);

public sealed record MovementRecord(Guid Id,Guid CheckoutId,string ItemName,MovementType Type,int Quantity,DateTimeOffset OccurredAt,string PerformedBy,string? Description);
