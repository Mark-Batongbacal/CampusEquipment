using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using CampusEquipment.Api.Middleware;
using CampusEquipment.Core.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;

namespace CampusEquipment.Tests;

public class ApiExceptionHandlerTests
{
    [Theory]
    [InlineData("validation", 400)]
    [InlineData("conflict", 409)]
    [InlineData("unexpected", 500)]
    public async Task Errors_return_expected_status_and_safe_consistent_envelope(string kind, int status)
    {
        using var services = new ServiceCollection().AddLogging().AddOptions().BuildServiceProvider();
        var context = new DefaultHttpContext { RequestServices = services };
        await using var body = new MemoryStream(); context.Response.Body = body;
        Exception exception = kind switch
        {
            "validation" => new ValidationException("The selected department does not exist."),
            "conflict" => new BusinessConflictException("Asset code already exists."),
            _ => new Exception("SECRET database details")
        };
        var handler = new ApiExceptionHandler(NullLogger<ApiExceptionHandler>.Instance);
        Assert.True(await handler.TryHandleAsync(context, exception, CancellationToken.None));
        Assert.Equal(status, context.Response.StatusCode);
        body.Position = 0;
        using var json = await JsonDocument.ParseAsync(body);
        Assert.False(json.RootElement.GetProperty("success").GetBoolean());
        Assert.Equal(JsonValueKind.Null, json.RootElement.GetProperty("data").ValueKind);
        var message = json.RootElement.GetProperty("message").GetString();
        if (status == 500) Assert.DoesNotContain("SECRET", message);
        else Assert.Equal(exception.Message, message);
    }
}
