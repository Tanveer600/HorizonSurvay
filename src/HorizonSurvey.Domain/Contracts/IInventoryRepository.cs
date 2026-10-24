using HorizonSurvey.Domain.Models;

namespace HorizonSurvey.Domain.Contracts;

public interface IInventoryRepository
{
    Task<InventorySnapshot> GetInventoryAsync(CancellationToken cancellationToken);
    Task<IReadOnlyList<OpenCheckout>> GetOpenCheckoutsAsync(CancellationToken cancellationToken);
    Task<IReadOnlyList<EquipmentCategory>> GetCategoriesAsync(CancellationToken cancellationToken);
    Task<EquipmentCategory> CreateCategoryAsync(EquipmentCategory category, CancellationToken cancellationToken);
    Task<EquipmentCategory> UpdateCategoryAsync(Guid id, string name, CancellationToken cancellationToken);
    Task DeleteCategoryAsync(Guid id, CancellationToken cancellationToken);
    Task<EquipmentAsset> RegisterEquipmentAsync(EquipmentAsset equipment, CancellationToken cancellationToken);
    Task<DroneApproval> RecordApprovalAsync(DroneApproval approval, CancellationToken cancellationToken);
    Task<CheckoutOutcome> CheckoutAsync(Checkout checkout,DateTimeOffset occurredAt,CancellationToken cancellationToken);
    Task<Checkout> ReturnAsync( Guid checkoutId,string returner,int quantity,string? damageDescription,int? damageQuantity,DateTimeOffset occurredAt,CancellationToken cancellationToken);
    Task<IReadOnlyList<MovementRecord>> GetHistoryAsync( DateTimeOffset from,DateTimeOffset to,CancellationToken cancellationToken);
    Task DeleteMovementAsync(Guid id, CancellationToken cancellationToken);
    Task<int> DeleteMovementsAsync(DateTimeOffset from, DateTimeOffset to, CancellationToken cancellationToken);
}
