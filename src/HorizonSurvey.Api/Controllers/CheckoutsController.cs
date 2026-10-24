using HorizonSurvey.Api.Contracts;
using HorizonSurvey.Application.Contracts;
using HorizonSurvey.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace HorizonSurvey.Api.Controllers;

[ApiController]
[Route("api/checkouts")]
public sealed class CheckoutsController(InventoryService inventoryService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetOpenCheckouts(CancellationToken cancellationToken) =>
        Ok(await inventoryService.GetOpenCheckoutsAsync(cancellationToken));

    [HttpPost]
    public async Task<IActionResult> Checkout(CheckoutRequest request,CancellationToken cancellationToken)
    {
        var outcome = await inventoryService.CheckoutAsync(
            new CheckoutCommand(
                request.Holder,
                request.ExpectedReturnAt,
                request.EquipmentAssetId,
                request.PooledStockId,
                request.Quantity,
                request.ApprovalId),
            cancellationToken);
        var checkout = outcome.Checkout;
        var receipt = new
        {
            checkout.Id,
            checkout.Holder,
            checkout.ExpectedReturnAt,
            checkout.Quantity,
            checkout.OutstandingQuantity,
            outcome.AssetAvailable,
            outcome.StockAvailableQuantity
        };

        return StatusCode(StatusCodes.Status201Created, receipt);
    }

    [HttpPost("{checkoutId:guid}/returns")]
    public async Task<IActionResult> Return( Guid checkoutId,ReturnRequest request,CancellationToken cancellationToken)
    {
        var checkout = await inventoryService.ReturnAsync(
            new ReturnCommand(
                checkoutId,
                request.Returner,
                request.Quantity,
                request.DamageDescription,
                request.DamageQuantity),
            cancellationToken);
        return Ok(new
        {
            checkout.Id,
            checkout.IsOpen,
            checkout.OutstandingQuantity,
            checkout.ReturnedAt
        });
    }
}
