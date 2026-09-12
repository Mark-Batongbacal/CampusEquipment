using CampusEquipment.Core.DTOs;

namespace CampusEquipment.Core.Services;

public interface IEquipmentService
{
    Task<List<EquipmentDto>> GetAllAsync(string? search = null, string? category = null,
        string? status = null, int? departmentId = null);
    Task<EquipmentDto?> GetByIdAsync(int id);
    Task<EquipmentDto> CreateAsync(CreateEquipmentDto dto);
    Task<bool> UpdateAsync(int id, UpdateEquipmentDto dto);
    Task<bool> DeleteAsync(int id);
    Task<bool> RetireAsync(int id);
}
