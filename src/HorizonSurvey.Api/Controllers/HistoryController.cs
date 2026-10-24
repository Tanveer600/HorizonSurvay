using HorizonSurvey.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace HorizonSurvey.Api.Controllers;

[ApiController]
[Route("api/history")]
public sealed class HistoryController(InventoryService inventoryService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] DateTimeOffset? from,[FromQuery] DateTimeOffset? to,CancellationToken cancellationToken)
    {
        var movements = await inventoryService.GetHistoryAsync(from, to, cancellationToken);
        return Ok(movements);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await inventoryService.DeleteMovementAsync(id, cancellationToken);
        return NoContent();
    }

    [HttpDelete]
    public async Task<IActionResult> DeleteRange(
        [FromQuery] DateTimeOffset? from,
        [FromQuery] DateTimeOffset? to,
        CancellationToken cancellationToken)
    {
        var deletedCount = await inventoryService.DeleteMovementsAsync(from, to, cancellationToken);
        return Ok(new { deletedCount });
    }
}
