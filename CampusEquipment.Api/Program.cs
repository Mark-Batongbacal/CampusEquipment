using CampusEquipment.Api.Middleware;
using CampusEquipment.Api.Models;
using CampusEquipment.Core.Repositories;
using CampusEquipment.Core.Services;
using Microsoft.AspNetCore.Mvc;
using CampusEquipment.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using CampusEquipment.Infrastructure.Entities;
using CampusEquipment.Infrastructure.Repositories;
using CampusEquipment.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is missing.")));

builder.Services.AddScoped<IEquipmentRepository<Equipment>, EquipmentRepository>();
builder.Services.AddScoped<IDepartmentRepository<Department>, DepartmentRepository>();

builder.Services.AddScoped<IEquipmentService, EquipmentService>();
builder.Services.AddScoped<IDepartmentService, DepartmentService>();

builder.Services.AddExceptionHandler<ApiExceptionHandler>();
builder.Services.AddProblemDetails();
builder.Services.AddControllers().ConfigureApiBehaviorOptions(options =>
{
    options.SuppressMapClientErrors = true;
    options.InvalidModelStateResponseFactory = context =>
    {
        var errors = context.ModelState.Values.SelectMany(value => value.Errors)
            .Select(error => string.IsNullOrWhiteSpace(error.ErrorMessage)
                ? "The request contains an invalid value." : error.ErrorMessage);
        var message = string.Join(" ", errors);
        context.HttpContext.RequestServices.GetRequiredService<ILoggerFactory>()
            .CreateLogger("RequestValidation").LogWarning("Validation failure: {Message}", message);
        return new BadRequestObjectResult(new ApiResponse<object>(false, message, null));
    };
});
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

app.UseExceptionHandler();
app.UseStatusCodePages(async context =>
{
    var response = context.HttpContext.Response;
    var message = response.StatusCode switch
    {
        404 => "The requested endpoint was not found.",
        405 => "The HTTP method is not supported for this endpoint.",
        415 => "The request content type is not supported.",
        _ => "The request could not be completed."
    };
    await response.WriteAsJsonAsync(new ApiResponse<object>(false, message, null));
});

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "Campus Equipment API v1");
    });
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
