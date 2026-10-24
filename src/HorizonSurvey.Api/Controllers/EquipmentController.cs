using HorizonSurvey.Api.Contracts;
using HorizonSurvey.Application.Contracts;
using HorizonSurvey.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace HorizonSurvey.Api.Controllers;

[ApiController]
[Route("api/equipment")]
public sealed class EquipmentController(InventoryService inventoryService) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Register(RegisterEquipmentRequest request,CancellationToken cancellationToken)
    {
        var equipment = await inventoryService.RegisterEquipmentAsync(
            new RegisterEquipmentCommand(request.Name, request.CategoryId),
            cancellationToken);
        return StatusCode(StatusCodes.Status201Created, equipment);
    }
}
