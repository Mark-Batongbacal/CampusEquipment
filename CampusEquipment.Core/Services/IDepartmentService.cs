using CampusEquipment.Core.DTOs;

namespace CampusEquipment.Core.Services;

public interface IDepartmentService
{
    Task<List<DepartmentDto>> GetAllAsync();
    Task<DepartmentDto?> GetByIdAsync(int id);
}
