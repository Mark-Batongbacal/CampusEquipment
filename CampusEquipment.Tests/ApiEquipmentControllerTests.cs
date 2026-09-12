using System.ComponentModel.DataAnnotations;
using CampusEquipment.Api.Models;
using CampusEquipment.Core.DTOs;
using CampusEquipment.Core.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using ApiController = CampusEquipment.Api.Controllers.EquipmentController;

namespace CampusEquipment.Tests;

public class ApiEquipmentControllerTests
{
    private readonly Mock<IEquipmentService> service = new();
    private ApiController Controller => new(service.Object, NullLogger<ApiController>.Instance);

    [Fact]
    public async Task Get_returns_success_envelope_and_passes_filters()
    {
        var rows = new List<EquipmentDto> { TestData.Equipment() };
        service.Setup(s => s.GetAllAsync("PC", "Computer", "Available", 1)).ReturnsAsync(rows);
        var result = Assert.IsType<OkObjectResult>((await Controller.GetAll("PC", "Computer", "Available", 1)).Result);
        var response = Assert.IsType<ApiResponse<List<EquipmentDto>>>(result.Value);
        Assert.True(response.Success); Assert.Same(rows, response.Data);
        service.Verify(s => s.GetAllAsync("PC", "Computer", "Available", 1), Times.Once);
    }

    [Fact]
    public async Task Create_returns_201_with_details_location_and_created_equipment()
    {
        var dto = TestData.Create(); var saved = TestData.Equipment();
        service.Setup(s => s.CreateAsync(dto)).ReturnsAsync(saved);
        var result = Assert.IsType<CreatedAtActionResult>((await Controller.Create(dto)).Result);
        Assert.Equal(201, result.StatusCode);
        Assert.Equal("GetById", result.ActionName);
        Assert.Equal(saved.EquipmentId, result.RouteValues!["id"]);
        Assert.Same(saved, Assert.IsType<ApiResponse<EquipmentDto>>(result.Value).Data);
        service.Verify(s => s.CreateAsync(dto), Times.Once);
    }

    [Theory]
    [InlineData("PC-001", 1, "Asset code already exists.")]
    [InlineData("PC-NEW", 999, "The selected department does not exist.")]
    public async Task Create_rejection_is_propagated_to_exception_handler(string code, int departmentId, string message)
    {
        var dto = TestData.Create(code, departmentId);
        service.Setup(s => s.CreateAsync(dto)).ThrowsAsync(new ValidationException(message));
        var error = await Assert.ThrowsAsync<ValidationException>(() => Controller.Create(dto));
        Assert.Equal(message, error.Message);
    }

    [Theory]
    [InlineData("Retired equipment cannot be assigned.")]
    [InlineData("Equipment under maintenance cannot be assigned.")]
    public async Task Assignment_rejection_is_propagated_to_exception_handler(string message)
    {
        var dto = TestData.Update();
        service.Setup(s => s.UpdateAsync(1, dto)).ThrowsAsync(new ValidationException(message));
        var error = await Assert.ThrowsAsync<ValidationException>(() => Controller.Update(1, dto));
        Assert.Equal(message, error.Message);
    }

    [Fact]
    public async Task Update_id_mismatch_returns_400_without_service_call()
    {
        Assert.IsType<BadRequestObjectResult>((await Controller.Update(2, TestData.Update())).Result);
        service.Verify(s => s.UpdateAsync(It.IsAny<int>(), It.IsAny<UpdateEquipmentDto>()), Times.Never);
    }

    [Fact]
    public async Task Get_missing_equipment_returns_404_error_envelope()
    {
        var result = Assert.IsType<NotFoundObjectResult>((await Controller.GetById(999)).Result);
        var response = Assert.IsType<ApiResponse<object>>(result.Value);
        Assert.False(response.Success); Assert.Null(response.Data);
    }
}
