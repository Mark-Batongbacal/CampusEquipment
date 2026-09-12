using System.ComponentModel.DataAnnotations;
using CampusEquipment.Api.Models;
using CampusEquipment.Core.DTOs;
using CampusEquipment.Core.Services;
using Microsoft.AspNetCore.Mvc;

namespace CampusEquipment.Api.Controllers;

[ApiController]
[Route("api/equipment")]
[Produces("application/json")]
public class EquipmentController(IEquipmentService service, ILogger<EquipmentController> logger) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<EquipmentDto>>>> GetAll(
        string? search = null, string? category = null, string? status = null,
        [Range(1, int.MaxValue)] int? departmentId = null)
    {
        var equipment = await service.GetAllAsync(search, category, status, departmentId);
        return Ok(new ApiResponse<List<EquipmentDto>>(true, "Equipment retrieved successfully.", equipment));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponse<EquipmentDto>>> GetById([Range(1, int.MaxValue)] int id)
    {
        var equipment = await service.GetByIdAsync(id);
        if (equipment is null) return EquipmentNotFound(id);
        return Ok(new ApiResponse<EquipmentDto>(true, "Equipment retrieved successfully.", equipment));
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<EquipmentDto>), StatusCodes.Status201Created)]
    public async Task<ActionResult<ApiResponse<EquipmentDto>>> Create(CreateEquipmentDto dto)
    {
        var equipment = await service.CreateAsync(dto);
        logger.LogInformation("Equipment {EquipmentId} created.", equipment.EquipmentId);
        return CreatedAtAction(nameof(GetById), new { id = equipment.EquipmentId },
            new ApiResponse<EquipmentDto>(true, "Equipment created successfully.", equipment));
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ApiResponse<object>>> Update([Range(1, int.MaxValue)] int id, UpdateEquipmentDto dto)
    {
        if (id != dto.EquipmentId)
        {
            logger.LogWarning("Validation failure: equipment route ID {EquipmentId} differs from body ID.", id);
            return BadRequest(new ApiResponse<object>(false, "Route ID must match EquipmentId in the request body.", null));
        }
        if (!await service.UpdateAsync(id, dto)) return EquipmentNotFound(id);
        logger.LogInformation("Equipment {EquipmentId} updated.", id);
        return Ok(new ApiResponse<object>(true, "Equipment updated successfully.", null));
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult<ApiResponse<object>>> Delete([Range(1, int.MaxValue)] int id)
    {
        if (!await service.DeleteAsync(id)) return EquipmentNotFound(id);
        logger.LogInformation("Equipment {EquipmentId} deleted.", id);
        return Ok(new ApiResponse<object>(true, "Equipment deleted successfully.", null));
    }

    [HttpPatch("{id:int}/retire")]
    public async Task<ActionResult<ApiResponse<object>>> Retire([Range(1, int.MaxValue)] int id)
    {
        if (!await service.RetireAsync(id)) return EquipmentNotFound(id);
        logger.LogInformation("Equipment {EquipmentId} retired.", id);
        return Ok(new ApiResponse<object>(true, "Equipment retired successfully.", null));
    }

    private NotFoundObjectResult EquipmentNotFound(int id)
    {
        logger.LogWarning("Equipment {EquipmentId} not found.", id);
        return NotFound(new ApiResponse<object>(false, "Equipment not found.", null));
    }
}
