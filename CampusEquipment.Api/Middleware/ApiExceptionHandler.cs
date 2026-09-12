using System.ComponentModel.DataAnnotations;
using CampusEquipment.Api.Models;
using CampusEquipment.Core.Exceptions;
using Microsoft.AspNetCore.Diagnostics;

namespace CampusEquipment.Api.Middleware;

public class ApiExceptionHandler(ILogger<ApiExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception,
        CancellationToken cancellationToken)
    {
        var (status, message) = exception switch
        {
            ValidationException => (StatusCodes.Status400BadRequest, exception.Message),
            BusinessConflictException => (StatusCodes.Status409Conflict, exception.Message),
            _ => (StatusCodes.Status500InternalServerError, "An unexpected error occurred. Please try again later.")
        };
        if (status == 500)
            logger.LogError(exception, "Unexpected exception processing {Method} {Path}.", context.Request.Method, context.Request.Path);
        else
            logger.LogWarning("Request rejected ({StatusCode}): {Message}", status, message);

        context.Response.StatusCode = status;
        await context.Response.WriteAsJsonAsync(new ApiResponse<object>(false, message, null), cancellationToken);
        return true;
    }
}
