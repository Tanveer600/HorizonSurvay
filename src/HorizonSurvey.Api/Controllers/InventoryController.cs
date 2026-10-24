using HorizonSurvey.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace HorizonSurvey.Api.Controllers;

[ApiController]
[Route("api/inventory")]
public sealed class InventoryController(InventoryService inventoryService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        var inventory = await inventoryService.GetInventoryAsync(cancellationToken);
        return Ok(inventory);
    }
}
