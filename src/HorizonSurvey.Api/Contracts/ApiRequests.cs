using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace HorizonSurvey.Api.Contracts;

public sealed record RegisterEquipmentRequest(
    [param: Required, StringLength(200, MinimumLength = 1)] string Name,
    [property: JsonRequired] Guid CategoryId);

public sealed record EquipmentCategoryRequest(
    [param: Required, StringLength(100, MinimumLength = 1)] string Name);

public sealed record RecordApprovalRequest(
    Guid EquipmentAssetId,
    [param: Required, StringLength(200, MinimumLength = 1)] string Holder,
    [param: Required, StringLength(100, MinimumLength = 1)] string ApprovedBy);

public sealed record CheckoutRequest(
    [param: Required, StringLength(200, MinimumLength = 1)] string Holder,
    DateTimeOffset ExpectedReturnAt,
    Guid? EquipmentAssetId,
    Guid? PooledStockId,
    int Quantity,
    Guid? ApprovalId);

public sealed record ReturnRequest(
    [param: Required, StringLength(200, MinimumLength = 1)] string Returner,
    int Quantity,
    [param: StringLength(1000)] string? DamageDescription,
    int? DamageQuantity);
