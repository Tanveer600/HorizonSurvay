using HorizonSurvey.Domain.Errors;
using Microsoft.AspNetCore.Mvc;

namespace HorizonSurvey.Api.Middleware;

public sealed class InventoryExceptionMiddleware(RequestDelegate next,ILogger<InventoryExceptionMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (InventoryException exception)
        {
            var (statusCode, title) = exception.Kind switch
            {
                InventoryErrorKind.NotFound => (StatusCodes.Status404NotFound, "Not found"),
                InventoryErrorKind.Conflict => (StatusCodes.Status409Conflict, "Inventory conflict"),
                _ => (StatusCodes.Status400BadRequest, "Invalid request")
            };
            context.Response.StatusCode = statusCode;
            await context.Response.WriteAsJsonAsync(new ProblemDetails
            {
                Status = statusCode,
                Title = title,
                Detail = exception.Message
            });
        }
        catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
        {
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Unhandled request failure.");
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            await context.Response.WriteAsJsonAsync(new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "Request failed",
                Detail = "The request could not be completed."
            });
        }
    }
}
