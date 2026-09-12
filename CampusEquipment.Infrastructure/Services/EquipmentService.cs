using System.ComponentModel.DataAnnotations;
using CampusEquipment.Core.DTOs;
using CampusEquipment.Core.Repositories;
using CampusEquipment.Core.Services;
using CampusEquipment.Infrastructure.Entities;
using Microsoft.Extensions.Logging;

namespace CampusEquipment.Infrastructure.Services;

public class EquipmentService : IEquipmentService
{
    private readonly IEquipmentRepository<Equipment> _equipmentRepository;
    private readonly IDepartmentRepository<Department> _departmentRepository;
    private readonly ILogger<EquipmentService> _logger;

    public EquipmentService(IEquipmentRepository<Equipment> equipmentRepository,
        IDepartmentRepository<Department> departmentRepository, ILogger<EquipmentService> logger)
    {
        _equipmentRepository = equipmentRepository;
        _departmentRepository = departmentRepository;
        _logger = logger;
    }

    public async Task<List<EquipmentDto>> GetAllAsync(string? search = null,
        string? category = null, string? status = null, int? departmentId = null)
    {
        var equipment = await _equipmentRepository.GetAllAsync();
        IEnumerable<Equipment> results = equipment;

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            results = results.Where(e =>
                e.AssetCode.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                e.Name.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                (e.Brand?.Contains(term, StringComparison.OrdinalIgnoreCase) ?? false));
        }

        if (!string.IsNullOrWhiteSpace(category))
            results = results.Where(e => string.Equals(e.Category, category.Trim(),
                StringComparison.OrdinalIgnoreCase));

        if (!string.IsNullOrWhiteSpace(status))
            results = results.Where(e => string.Equals(e.Status, status.Trim(),
                StringComparison.OrdinalIgnoreCase));

        if (departmentId.HasValue)
            results = results.Where(e => e.DepartmentId == departmentId.Value);

        var departments = await _departmentRepository.GetAllAsync();
        var departmentNames = departments.ToDictionary(d => d.DepartmentId, d => d.Name);

        return results.Select(e => ToDto(e,
            departmentNames.GetValueOrDefault(e.DepartmentId, string.Empty))).ToList();
    }

    public async Task<EquipmentDto?> GetByIdAsync(int id)
    {
        var equipment = await _equipmentRepository.GetByIdAsync(id);
        if (equipment is null)
        {
            _logger.LogInformation("Equipment {EquipmentId} was not found.", id);
            return null;
        }

        var department = await _departmentRepository.GetByIdAsync(equipment.DepartmentId);
        return ToDto(equipment, department?.Name ?? string.Empty);
    }

    public async Task<EquipmentDto> CreateAsync(CreateEquipmentDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto);
        ValidateInput(dto);
        var department = await ValidateReferencesAsync(dto.AssetCode, dto.DepartmentId);

        var equipment = new Equipment
        {
            AssetCode = dto.AssetCode.Trim(),
            Name = dto.Name.Trim(),
            Category = dto.Category.Trim(),
            Brand = dto.Brand?.Trim(),
            Model = dto.Model?.Trim(),
            PurchaseDate = dto.PurchaseDate,
            Status = dto.Status,
            DepartmentId = dto.DepartmentId
        };

        await _equipmentRepository.AddAsync(equipment);
        _logger.LogInformation("Equipment {EquipmentId} was created.", equipment.EquipmentId);
        return ToDto(equipment, department.Name);
    }

    public async Task<bool> UpdateAsync(int id, UpdateEquipmentDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto);
        var equipment = await _equipmentRepository.GetByIdAsync(id);
        if (equipment is null)
        {
            _logger.LogInformation("Equipment {EquipmentId} was not found.", id);
            return false;
        }

        ValidateInput(dto);
        if (dto.EquipmentId != id)
        {
            throw new ValidationException("Equipment ID does not match the requested record.");
        }

        if (dto.Status == "Assigned" &&
            (equipment.Status == "Retired" || equipment.Status == "UnderMaintenance"))
        {
            _logger.LogWarning("Assignment rejected for equipment {EquipmentId}.", id);
            throw new ValidationException("Retired equipment or equipment under maintenance cannot be assigned.");
        }

        await ValidateReferencesAsync(dto.AssetCode, dto.DepartmentId, id);

        equipment.AssetCode = dto.AssetCode.Trim();
        equipment.Name = dto.Name.Trim();
        equipment.Category = dto.Category.Trim();
        equipment.Brand = dto.Brand?.Trim();
        equipment.Model = dto.Model?.Trim();
        equipment.PurchaseDate = dto.PurchaseDate;
        equipment.Status = dto.Status;
        equipment.DepartmentId = dto.DepartmentId;

        var updated = await _equipmentRepository.UpdateAsync(equipment);
        if (updated)
            _logger.LogInformation("Equipment {EquipmentId} was updated.", id);
        return updated;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var deleted = await _equipmentRepository.DeleteAsync(id);
        if (deleted)
            _logger.LogInformation("Equipment {EquipmentId} was deleted.", id);
        else
            _logger.LogInformation("Equipment {EquipmentId} was not found.", id);
        return deleted;
    }

    public async Task<bool> RetireAsync(int id)
    {
        var equipment = await _equipmentRepository.GetByIdAsync(id);
        if (equipment is null)
        {
            _logger.LogInformation("Equipment {EquipmentId} was not found.", id);
            return false;
        }

        equipment.Status = "Retired";
        var updated = await _equipmentRepository.UpdateAsync(equipment);
        if (updated)
            _logger.LogInformation("Equipment {EquipmentId} was retired.", id);
        return updated;
    }

    private void ValidateInput(object dto)
    {
        try
        {
            Validator.ValidateObject(dto, new ValidationContext(dto), validateAllProperties: true);
        }
        catch (ValidationException)
        {
            _logger.LogWarning("Equipment input validation failed.");
            throw;
        }
    }

    private async Task<Department> ValidateReferencesAsync(string assetCode, int departmentId,
        int? equipmentId = null)
    {
        var equipment = await _equipmentRepository.GetAllAsync();
        if (equipment.Any(e => e.EquipmentId != equipmentId &&
            string.Equals(e.AssetCode.Trim(), assetCode.Trim(), StringComparison.OrdinalIgnoreCase)))
        {
            _logger.LogWarning("Equipment creation or update rejected: duplicate asset code.");
            throw new ValidationException("Asset code already exists.");
        }

        var department = await _departmentRepository.GetByIdAsync(departmentId);
        if (department is null)
        {
            _logger.LogWarning("Department {DepartmentId} was not found.", departmentId);
            throw new ValidationException("The selected department does not exist.");
        }

        return department;
    }

    private static EquipmentDto ToDto(Equipment equipment, string departmentName)
    {
        return new EquipmentDto
        {
            EquipmentId = equipment.EquipmentId,
            AssetCode = equipment.AssetCode,
            Name = equipment.Name,
            Category = equipment.Category,
            Brand = equipment.Brand,
            Model = equipment.Model,
            PurchaseDate = equipment.PurchaseDate,
            Status = equipment.Status,
            DepartmentId = equipment.DepartmentId,
            DepartmentName = departmentName
        };
    }
}
