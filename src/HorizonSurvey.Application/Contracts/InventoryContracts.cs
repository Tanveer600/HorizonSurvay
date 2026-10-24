using HorizonSurvey.Domain.Models;

namespace HorizonSurvey.Application.Contracts;

public sealed record RegisterEquipmentCommand(string Name, Guid CategoryId);
public sealed record CreateCategoryCommand(string Name);
public sealed record UpdateCategoryCommand(Guid Id, string Name);

public sealed record RecordApprovalCommand(Guid EquipmentAssetId, string Holder, string ApprovedBy);

public sealed record CheckoutCommand(string Holder,DateTimeOffset ExpectedReturnAt,Guid? EquipmentAssetId,Guid? PooledStockId,int Quantity,Guid? ApprovalId);

public sealed record ReturnCommand(Guid CheckoutId,string Returner,int Quantity,string? DamageDescription,int? DamageQuantity = null);
