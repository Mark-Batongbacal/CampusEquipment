using System.ComponentModel.DataAnnotations;
using CampusEquipment.Core.DTOs;
using CampusEquipment.Core.Services;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace CampusEquipment.Tests;

public class WebEquipmentControllerTests
{
    private readonly Mock<IEquipmentService> equipment = new();
    private readonly Mock<IDepartmentService> departments = new();

    public WebEquipmentControllerTests() => departments.Setup(s => s.GetAllAsync())
        .ReturnsAsync(new List<DepartmentDto> { new() { DepartmentId = 1, Name = "IT" } });

    [Fact]
    public async Task Index_passes_filters_and_returns_equipment_for_view()
    {
        var rows = new List<EquipmentDto> { TestData.Equipment() };
        equipment.Setup(s => s.GetAllAsync("PC", "Computer", "Available", 1)).ReturnsAsync(rows);
        var controller = TestData.Web(equipment.Object, departments.Object);
        var view = Assert.IsType<ViewResult>(await controller.Index("PC", "Computer", "Available", 1));
        Assert.Same(rows, view.Model);
        Assert.NotNull(controller.ViewData["Departments"]);
        equipment.Verify(s => s.GetAllAsync("PC", "Computer", "Available", 1), Times.Once);
    }

    [Fact]
    public async Task Create_redirects_and_sets_success_message()
    {
        var dto = TestData.Create();
        equipment.Setup(s => s.CreateAsync(dto)).ReturnsAsync(TestData.Equipment());
        var controller = TestData.Web(equipment.Object, departments.Object);
        var result = Assert.IsType<RedirectToActionResult>(await controller.Create(dto));
        Assert.Equal("Index", result.ActionName);
        Assert.NotNull(controller.TempData["SuccessMessage"]);
        equipment.Verify(s => s.CreateAsync(dto), Times.Once);
    }

    [Theory]
    [InlineData("PC-001", 1, "Asset code already exists.")]
    [InlineData("PC-NEW", 999, "The selected department does not exist.")]
    public async Task Create_rejection_preserves_input_and_displays_error(string code, int departmentId, string message)
    {
        var dto = TestData.Create(code, departmentId);
        equipment.Setup(s => s.CreateAsync(dto)).ThrowsAsync(new ValidationException(message));
        var controller = TestData.Web(equipment.Object, departments.Object);
        Assert.Same(dto, Assert.IsType<ViewResult>(await controller.Create(dto)).Model);
        Assert.Contains(controller.ModelState.Values.SelectMany(v => v.Errors), e => e.ErrorMessage == message);
        Assert.NotNull(controller.ViewData["Departments"]);
        Assert.Null(controller.TempData["SuccessMessage"]);
    }

    [Theory]
    [InlineData("Retired equipment cannot be assigned.")]
    [InlineData("Equipment under maintenance cannot be assigned.")]
    public async Task Edit_displays_service_assignment_rejection(string message)
    {
        var dto = TestData.Update();
        equipment.Setup(s => s.UpdateAsync(1, dto)).ThrowsAsync(new ValidationException(message));
        var controller = TestData.Web(equipment.Object, departments.Object);
        Assert.Same(dto, Assert.IsType<ViewResult>(await controller.Edit(1, dto)).Model);
        Assert.Contains(controller.ModelState.Values.SelectMany(v => v.Errors), e => e.ErrorMessage == message);
        Assert.NotNull(controller.ViewData["Departments"]);
    }

    [Fact]
    public async Task Invalid_model_does_not_call_create_service()
    {
        var controller = TestData.Web(equipment.Object, departments.Object);
        controller.ModelState.AddModelError("Name", "Required");
        Assert.IsType<ViewResult>(await controller.Create(TestData.Create()));
        equipment.Verify(s => s.CreateAsync(It.IsAny<CreateEquipmentDto>()), Times.Never);
    }

    [Fact]
    public async Task Edit_id_mismatch_does_not_call_service()
    {
        var controller = TestData.Web(equipment.Object, departments.Object);
        Assert.IsType<BadRequestResult>(await controller.Edit(2, TestData.Update()));
        equipment.Verify(s => s.UpdateAsync(It.IsAny<int>(), It.IsAny<UpdateEquipmentDto>()), Times.Never);
    }

    [Fact]
    public async Task Details_missing_equipment_returns_not_found()
    {
        Assert.IsType<NotFoundResult>(await TestData.Web(equipment.Object, departments.Object).Details(999));
    }
}
