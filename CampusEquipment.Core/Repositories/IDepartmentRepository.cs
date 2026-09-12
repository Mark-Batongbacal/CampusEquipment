namespace CampusEquipment.Core.Repositories;

/// <summary>
/// Data access contract. Infrastructure supplies the scaffolded entity type.
/// </summary>
public interface IDepartmentRepository<T> where T : class
{
    Task<List<T>> GetAllAsync();
    Task<T?> GetByIdAsync(int id);
    Task AddAsync(T entity);

    // Returns false when the record does not exist.
    Task<bool> UpdateAsync(T entity);
    Task<bool> DeleteAsync(int id);
}
