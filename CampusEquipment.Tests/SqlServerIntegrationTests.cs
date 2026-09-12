using CampusEquipment.Api.Models;
using CampusEquipment.Core.DTOs;
using CampusEquipment.Infrastructure.Data;
using CampusEquipment.Infrastructure.Entities;
using CampusEquipment.Infrastructure.Repositories;
using CampusEquipment.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using ApiController = CampusEquipment.Api.Controllers.EquipmentController;

namespace CampusEquipment.Tests;

public sealed class SqlServerFactAttribute : FactAttribute
{
    public SqlServerFactAttribute()
    {
        if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("CAMPUS_EQUIPMENT_TEST_CONNECTION")))
            Skip = "Set CAMPUS_EQUIPMENT_TEST_CONNECTION to a dedicated SQL Server test database with the scaffolded schema.";
    }
}

public class SqlServerIntegrationTests
{
    [SqlServerFact]
    [Trait("Category", "SqlServer")]
    public Task Api_creation_is_stored_in_sql_server_and_returned_by_list() => VerifyRoundTrip(true);

    [SqlServerFact]
    [Trait("Category", "SqlServer")]
    public Task Mvc_creation_is_stored_in_sql_server_and_returned_to_view() => VerifyRoundTrip(false);

    private static async Task VerifyRoundTrip(bool api)
    {
        var connection = Environment.GetEnvironmentVariable("CAMPUS_EQUIPMENT_TEST_CONNECTION")!;
        var settings = new SqlConnectionStringBuilder(connection);
        Assert.Contains("test", settings.InitialCatalog.ToLowerInvariant());
        var options = new DbContextOptionsBuilder<AppDbContext>().UseSqlServer(connection).Options;
        await using var db = new AppDbContext(options);
        await using var transaction = await db.Database.BeginTransactionAsync();
        try
        {
            var department = new Department { Name = "Automated test department" };
            db.Departments.Add(department);
            await db.SaveChangesAsync();
            var service = new EquipmentService(new EquipmentRepository(db), new DepartmentRepository(db),
                NullLogger<EquipmentService>.Instance);
            var dto = TestData.Create("TEST-" + Guid.NewGuid().ToString("N"), department.DepartmentId);
            if (api)
            {
                var controller = new ApiController(service, NullLogger<ApiController>.Instance);
                var created = Assert.IsType<CreatedAtActionResult>((await controller.Create(dto)).Result);
                Assert.True(Assert.IsType<ApiResponse<EquipmentDto>>(created.Value).Data!.EquipmentId > 0);
                var list = Assert.IsType<OkObjectResult>((await controller.GetAll(search: dto.AssetCode)).Result);
                Assert.Equal(dto.AssetCode, Assert.Single(Assert.IsType<ApiResponse<List<EquipmentDto>>>(list.Value).Data!).AssetCode);
            }
            else
            {
                var controller = TestData.Web(service, new DepartmentService(new DepartmentRepository(db)));
                Assert.IsType<RedirectToActionResult>(await controller.Create(dto));
                var view = Assert.IsType<ViewResult>(await controller.Index(dto.AssetCode, null, null, null));
                Assert.Equal(dto.AssetCode, Assert.Single(Assert.IsType<List<EquipmentDto>>(view.Model)).AssetCode);
            }
            // Clear tracking so the assertion reads SQL Server rather than the inserted in-memory entity.
            db.ChangeTracker.Clear();
            var stored = await db.Equipment.AsNoTracking().SingleAsync(e => e.AssetCode == dto.AssetCode);
            Assert.Equal(dto.Name, stored.Name);
            Assert.Equal(dto.Category, stored.Category);
            Assert.Equal(dto.Brand, stored.Brand);
            Assert.Equal(dto.Model, stored.Model);
            Assert.Equal(dto.PurchaseDate, stored.PurchaseDate);
            Assert.Equal(dto.Status, stored.Status);
            Assert.Equal(dto.DepartmentId, stored.DepartmentId);
        }
        finally
        {
            await transaction.RollbackAsync();
        }
    }
}
