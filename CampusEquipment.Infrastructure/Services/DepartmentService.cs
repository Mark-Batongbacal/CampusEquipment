using CampusEquipment.Core.DTOs;
using CampusEquipment.Core.Repositories;
using CampusEquipment.Core.Services;
using CampusEquipment.Infrastructure.Entities;

namespace CampusEquipment.Infrastructure.Services;

public class DepartmentService : IDepartmentService
{
    private readonly IDepartmentRepository<Department> _departmentRepository;

    public DepartmentService(IDepartmentRepository<Department> departmentRepository)
    {
        _departmentRepository = departmentRepository;
    }

    public async Task<List<DepartmentDto>> GetAllAsync()
    {
        var departments = await _departmentRepository.GetAllAsync();
        return departments.Select(ToDto).ToList();
    }

    public async Task<DepartmentDto?> GetByIdAsync(int id)
    {
        var department = await _departmentRepository.GetByIdAsync(id);
        return department is null ? null : ToDto(department);
    }

    private static DepartmentDto ToDto(Department department)
    {
        return new DepartmentDto
        {
            DepartmentId = department.DepartmentId,
            Name = department.Name,
            Description = department.Description
        };
    }
}
