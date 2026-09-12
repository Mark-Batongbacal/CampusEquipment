using System.ComponentModel.DataAnnotations;
using CampusEquipment.Api.Models;
using CampusEquipment.Core.DTOs;
using CampusEquipment.Core.Services;
using Microsoft.AspNetCore.Mvc;

namespace CampusEquipment.Api.Controllers;

[ApiController]
[Route("api/departments")]
[Produces("application/json")]
public class DepartmentController(IDepartmentService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<DepartmentDto>>>> GetAll()
    {
        return Ok(new ApiResponse<List<DepartmentDto>>(true, "Departments retrieved successfully.",
            await service.GetAllAsync()));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponse<DepartmentDto>>> GetById([Range(1, int.MaxValue)] int id)
    {
        var department = await service.GetByIdAsync(id);
        if (department is null)
            return NotFound(new ApiResponse<DepartmentDto>(false, "Department not found.", null));
        return Ok(new ApiResponse<DepartmentDto>(true, "Department retrieved successfully.", department));
    }
}
