using System.ComponentModel.DataAnnotations;
using CampusEquipment.Core.Repositories;
using CampusEquipment.Infrastructure.Entities;
using CampusEquipment.Infrastructure.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace CampusEquipment.Tests;

public class EquipmentServiceTests
{
    private readonly Mock<IEquipmentRepository<Equipment>> equipment = new();
    private readonly Mock<IDepartmentRepository<Department>> departments = new();
    private EquipmentService Service => new(equipment.Object, departments.Object, NullLogger<EquipmentService>.Instance);

    public EquipmentServiceTests()
    {
        equipment.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Equipment>());
        departments.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new Department { DepartmentId = 1, Name = "IT" });
    }

    [Fact]
    public async Task Display_maps_equipment_and_department_name()
    {
        equipment.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Equipment> { new() {
            EquipmentId = 1, AssetCode = "PC-001", Name = "Desktop", Category = "Computer",
            Brand = "Dell", Status = "Available", DepartmentId = 1 } });
        departments.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Department> { new() { DepartmentId = 1, Name = "IT" } });
        var result = Assert.Single(await Service.GetAllAsync());
        Assert.Equal("PC-001", result.AssetCode);
        Assert.Equal("IT", result.DepartmentName);
    }

    [Fact]
    public async Task Create_sends_fields_to_repository_and_returns_generated_id()
    {
        var dto = TestData.Create();
        equipment.Setup(r => r.AddAsync(It.IsAny<Equipment>())).Callback<Equipment>(e => e.EquipmentId = 42).Returns(Task.CompletedTask);
        var result = await Service.CreateAsync(dto);
        Assert.Equal(42, result.EquipmentId);
        Assert.Equal("IT", result.DepartmentName);
        equipment.Verify(r => r.AddAsync(It.Is<Equipment>(e => e.AssetCode == dto.AssetCode &&
            e.Name == dto.Name && e.Category == dto.Category && e.Brand == dto.Brand &&
            e.Model == dto.Model && e.PurchaseDate == dto.PurchaseDate &&
            e.Status == dto.Status && e.DepartmentId == dto.DepartmentId)), Times.Once);
    }

    [Theory]
    [InlineData("PC-001")]
    [InlineData(" pc-001 ")]
    public async Task Duplicate_asset_code_is_rejected_without_insert(string code)
    {
        equipment.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Equipment> { new() { EquipmentId = 1, AssetCode = "PC-001" } });
        var error = await Assert.ThrowsAsync<ValidationException>(() => Service.CreateAsync(TestData.Create(code)));
        Assert.Contains("already exists", error.Message);
        equipment.Verify(r => r.AddAsync(It.IsAny<Equipment>()), Times.Never);
    }

    [Fact]
    public async Task Nonexistent_department_is_rejected_without_insert()
    {
        var error = await Assert.ThrowsAsync<ValidationException>(() => Service.CreateAsync(TestData.Create(departmentId: 999)));
        Assert.Contains("department does not exist", error.Message);
        equipment.Verify(r => r.AddAsync(It.IsAny<Equipment>()), Times.Never);
    }

    [Theory]
    [InlineData("Retired")]
    [InlineData("UnderMaintenance")]
    public async Task Restricted_equipment_cannot_be_assigned(string status)
    {
        var existing = new Equipment { EquipmentId = 1, AssetCode = "PC-001", Status = status };
        equipment.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(existing);
        var error = await Assert.ThrowsAsync<ValidationException>(() => Service.UpdateAsync(1, TestData.Update()));
        Assert.Contains("cannot be assigned", error.Message);
        Assert.Equal(status, existing.Status);
        equipment.Verify(r => r.UpdateAsync(It.IsAny<Equipment>()), Times.Never);
    }

    [Fact]
    public async Task Available_equipment_can_be_assigned()
    {
        equipment.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new Equipment { EquipmentId = 1, AssetCode = "PC-001", Status = "Available" });
        equipment.Setup(r => r.UpdateAsync(It.IsAny<Equipment>())).ReturnsAsync(true);
        Assert.True(await Service.UpdateAsync(1, TestData.Update()));
        equipment.Verify(r => r.UpdateAsync(It.Is<Equipment>(e => e.Status == "Assigned")), Times.Once);
    }

    [Fact]
    public async Task Missing_required_name_is_rejected_without_insert()
    {
        var dto = TestData.Create(); dto.Name = "";
        await Assert.ThrowsAsync<ValidationException>(() => Service.CreateAsync(dto));
        equipment.Verify(r => r.AddAsync(It.IsAny<Equipment>()), Times.Never);
    }
}
