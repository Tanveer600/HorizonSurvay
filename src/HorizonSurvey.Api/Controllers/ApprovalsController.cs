using HorizonSurvey.Api.Contracts;
using HorizonSurvey.Application.Contracts;
using HorizonSurvey.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace HorizonSurvey.Api.Controllers;

[ApiController]
[Route("api/approvals")]
public sealed class ApprovalsController(InventoryService inventoryService) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Record(
        RecordApprovalRequest request,
        CancellationToken cancellationToken)
    {
        var approval = await inventoryService.RecordApprovalAsync(
            new RecordApprovalCommand(request.EquipmentAssetId, request.Holder, request.ApprovedBy),
            cancellationToken);
        return StatusCode(StatusCodes.Status201Created, approval);
    }
}
