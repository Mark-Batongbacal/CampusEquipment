using CampusEquipment.Core.Repositories;
using CampusEquipment.Infrastructure.Data;
using CampusEquipment.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;

namespace CampusEquipment.Infrastructure.Repositories;

public class EquipmentRepository : IEquipmentRepository<Equipment>
{
    private readonly AppDbContext _context;

    public EquipmentRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Equipment>> GetAllAsync()
    {
        return await _context.Equipment.AsNoTracking().ToListAsync();
    }

    public async Task<Equipment?> GetByIdAsync(int id)
    {
        return await _context.Equipment.AsNoTracking()
            .FirstOrDefaultAsync(entity => entity.EquipmentId == id);
    }

    public async Task AddAsync(Equipment entity)
    {
        _context.Entry(entity).State = EntityState.Added;
        await _context.SaveChangesAsync();
    }

    public async Task<bool> UpdateAsync(Equipment entity)
    {
        var existing = await _context.Equipment.FindAsync(entity.EquipmentId);

        if (existing is null)
        {
            return false;
        }

        existing.AssetCode = entity.AssetCode;
        existing.Name = entity.Name;
        existing.Category = entity.Category;
        existing.Brand = entity.Brand;
        existing.Model = entity.Model;
        existing.PurchaseDate = entity.PurchaseDate;
        existing.Status = entity.Status;
        existing.DepartmentId = entity.DepartmentId;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var existing = await _context.Equipment.FindAsync(id);

        if (existing is null)
        {
            return false;
        }

        _context.Equipment.Remove(existing);
        await _context.SaveChangesAsync();
        return true;
    }
}
