using HorizonSurvey.Api.Contracts;
using HorizonSurvey.Application.Contracts;
using HorizonSurvey.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace HorizonSurvey.Api.Controllers;

[ApiController]
[Route("api/categories")]
public sealed class EquipmentCategoriesController(InventoryService inventoryService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken cancellationToken) =>
        Ok(await inventoryService.GetCategoriesAsync(cancellationToken));

    [HttpPost]
    public async Task<IActionResult> Create(EquipmentCategoryRequest request, CancellationToken cancellationToken)
    {
        var category = await inventoryService.CreateCategoryAsync(new CreateCategoryCommand(request.Name), cancellationToken);
        return StatusCode(StatusCodes.Status201Created, category);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, EquipmentCategoryRequest request, CancellationToken cancellationToken) =>
        Ok(await inventoryService.UpdateCategoryAsync(new UpdateCategoryCommand(id, request.Name), cancellationToken));

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await inventoryService.DeleteCategoryAsync(id, cancellationToken);
        return NoContent();
    }
}
