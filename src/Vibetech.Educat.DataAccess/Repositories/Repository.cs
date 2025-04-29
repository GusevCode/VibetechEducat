using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Vibetech.Educat.DataAccess.Data;
using Vibetech.Educat.DataAccess.Interfaces;
using Vibetech.Educat.Common.Interfaces;

namespace Vibetech.Educat.DataAccess.Repositories;

public class Repository<T, TKey> : IRepository<T, TKey> where T : class
{
    protected readonly EducatDbContext _context;
    protected readonly DbSet<T> _dbSet;

    public Repository(EducatDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    public async Task<T?> GetByIdAsync(TKey id)
    {
        return await _dbSet.FindAsync(id);
    }

    public async Task<IEnumerable<T>> GetAllAsync()
    {
        return await _dbSet.ToListAsync();
    }

    public async Task<IEnumerable<T>> GetAllWithIncludesAsync(params Expression<Func<T, object>>[] includes)
    {
        IQueryable<T> query = _dbSet;
        foreach (var include in includes)
        {
            query = query.Include(include);
        }
        return await query.ToListAsync();
    }

    public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
    {
        return await _dbSet.Where(predicate).ToListAsync();
    }

    public async Task<T> AddAsync(T entity)
    {
        await _dbSet.AddAsync(entity);
        return entity;
    }

    public async Task<T> UpdateAsync(T entity)
    {
        _dbSet.Update(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task<T?> UpdateByEntityId(TKey entityId, T newEntity)
    {
        var entity = await GetByIdAsync(entityId);
        if (entity == null) return entity;
        entity = newEntity;
        _dbSet.Update(entity);
        return entity;
    }

    public async Task<bool> DeleteAsync(TKey id)
    {
        var entity = await GetByIdAsync(id);
        if (entity == null) return false;
        _dbSet.Remove(entity);
        await _context.SaveChangesAsync();
        return true;
    }

    public void Remove(T entity)
    {
        _dbSet.Remove(entity);
    }

    public async Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate)
    {
        return await _dbSet.AnyAsync(predicate);
    }
}

public class Repository<T> : Repository<T, int>, IRepository<T> where T : class
{
    public Repository(EducatDbContext context) : base(context)
    {
    }
} 