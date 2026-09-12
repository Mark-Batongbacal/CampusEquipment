using CampusEquipment.Core.DTOs;
using CampusEquipment.Core.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;
using WebController = CampusEquipment.Web.Controllers.EquipmentController;

namespace CampusEquipment.Tests;

internal static class TestData
{
    public static CreateEquipmentDto Create(string code = "PC-NEW", int departmentId = 1) => new()
    {
        AssetCode = code, Name = "Desktop Computer", Category = "Computer",
        Brand = "Dell", Model = "OptiPlex", PurchaseDate = new DateOnly(2025, 1, 15),
        Status = "Available", DepartmentId = departmentId
    };

    public static UpdateEquipmentDto Update(string status = "Assigned") => new()
    {
        EquipmentId = 1, AssetCode = "PC-001", Name = "Desktop Computer",
        Category = "Computer", Status = status, DepartmentId = 1
    };

    public static EquipmentDto Equipment() => new()
    {
        EquipmentId = 1, AssetCode = "PC-001", Name = "Desktop Computer",
        Category = "Computer", Status = "Available", DepartmentId = 1,
        DepartmentName = "Information Technology"
    };

    public static WebController Web(IEquipmentService equipment, IDepartmentService departments)
    {
        var controller = new WebController(equipment, departments);
        controller.TempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>());
        return controller;
    }
}
