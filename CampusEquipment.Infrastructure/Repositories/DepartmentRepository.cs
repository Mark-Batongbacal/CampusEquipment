using CampusEquipment.Core.Repositories;
using CampusEquipment.Infrastructure.Data;
using CampusEquipment.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;

namespace CampusEquipment.Infrastructure.Repositories;

public class DepartmentRepository : IDepartmentRepository<Department>
{
    private readonly AppDbContext _context;

    public DepartmentRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Department>> GetAllAsync()
    {
        return await _context.Departments.AsNoTracking().ToListAsync();
    }

    public async Task<Department?> GetByIdAsync(int id)
    {
        return await _context.Departments.AsNoTracking()
            .FirstOrDefaultAsync(entity => entity.DepartmentId == id);
    }

    public async Task AddAsync(Department entity)
    {
        // Add only this row; do not insert related navigation entities.
        _context.Entry(entity).State = EntityState.Added;
        await _context.SaveChangesAsync();
    }

    public async Task<bool> UpdateAsync(Department entity)
    {
        var existing = await _context.Departments.FindAsync(entity.DepartmentId);

        if (existing is null)
        {
            return false;
        }

        existing.Name = entity.Name;
        existing.Description = entity.Description;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var existing = await _context.Departments.FindAsync(id);

        if (existing is null)
        {
            return false;
        }

        _context.Departments.Remove(existing);
        await _context.SaveChangesAsync();
        return true;
    }
}
