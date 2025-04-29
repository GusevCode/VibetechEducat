using System.Linq.Expressions;

namespace Vibetech.Educat.Common.Interfaces;

public interface IRepository<T, TKey> where T : class
{
    Task<T?> GetByIdAsync(TKey id);
    Task<IEnumerable<T>> GetAllAsync();
    Task<IEnumerable<T>> GetAllWithIncludesAsync(params Expression<Func<T, object>>[] includes);
    Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
    Task<T> AddAsync(T entity);
    Task<T> UpdateAsync(T entity);
    Task<T?> UpdateByEntityId(TKey entityId, T newEntity);
    Task<bool> DeleteAsync(TKey id);
    Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate);
    void Remove(T entity);
}

public interface IRepository<T> : IRepository<T, int> where T : class
{
} 