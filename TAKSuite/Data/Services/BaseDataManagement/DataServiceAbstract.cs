using TAKSuite.Data.Models;
using TAKSuite.Data.Services.BaseDataManagement;
using TAKSuite.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using Microsoft.Extensions.Caching.Memory;

public abstract class DataServiceAbstract<T> : IService, IDataService<T> where T : class, IGuidModel, new()
{
    protected readonly IDbContextFactory<ApplicationDbContext> _factory;
    private readonly Func<ApplicationDbContext, DbSet<T>> _dbSetSelector;
    public Expression<Func<T, object>>[]? Includes { get; set; }

    protected readonly IMemoryCache _cache;
    private readonly string _cacheKey = typeof(T).Name;

    protected DataServiceAbstract(IDbContextFactory<ApplicationDbContext> factory, Func<ApplicationDbContext, DbSet<T>> dbSetSelector, IMemoryCache cache)
    {
        _factory = factory;
        _dbSetSelector = dbSetSelector;
        _cache = cache;
    }

    public virtual T CreateNew() => new T();

    public virtual async Task<T> AddAsync(T element)
    {
        try
        {
            if (element == null) return null;
            element.Id = Guid.NewGuid();
            using var ctx = _factory.CreateDbContext();
            _dbSetSelector(ctx).Add(element);
            await ctx.SaveChangesAsync();
            _cache.Remove(_cacheKey);
            return element;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Errore nella richiesta: {ex.Message}");
            return null;
        }
    }

    public virtual async Task<bool> DeleteAsync(Guid id)
    {
        try
        {
            using var ctx = _factory.CreateDbContext();
            var dbSet = _dbSetSelector(ctx);
            var item = await dbSet.FindAsync(id);
            if (item == null) return false;
            dbSet.Remove(item);
            await ctx.SaveChangesAsync();
            _cache.Remove(_cacheKey);
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Errore nella richiesta: {ex.Message}");
            return false;
        }
    }

    public virtual async Task<List<T>> GetAllAsync()
    {
        try
        {
            using var ctx = _factory.CreateDbContext();
            IQueryable<T> query = _dbSetSelector(ctx);
            if (Includes != null)
            {
                foreach (var include in Includes)
                    query = query.Include(include);
            }
            var data = await query.ToListAsync();
            _cache.Set(_cacheKey, data, TimeSpan.FromMinutes(10));
            return data;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Errore nella richiesta: {ex.Message}");
            return new List<T>();
        }
    }

    public virtual async Task<T?> GetAsync(Guid id)
    {
        try
        {
            using var ctx = _factory.CreateDbContext();
            IQueryable<T> query = _dbSetSelector(ctx);
            if (Includes != null)
            {
                foreach (var include in Includes)
                    query = query.Include(include);
            }
            var item = await query.FirstOrDefaultAsync(r => r.Id == id);
            if (item != null)
                _cache.Set($"{_cacheKey}_{id}", item, TimeSpan.FromMinutes(10));
            return item;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Errore nella richiesta: {ex.Message}");
            return null;
        }
    }

    public virtual async Task<T> UpdateAsync(T element)
    {
        try
        {
            if (element == null) return null;
            using var ctx = _factory.CreateDbContext();
            var dbSet = _dbSetSelector(ctx);
            var existingItem = await dbSet.FindAsync(element.Id);
            if (existingItem == null) return null;
            EntityUpdater.UpdateEntity(existingItem, element);
            await ctx.SaveChangesAsync();
            _cache.Remove(_cacheKey);
            _cache.Remove($"{_cacheKey}_{element.Id}");
            return element;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Errore nella richiesta: {ex.Message}");
            return null;
        }
    }

    public virtual async Task<T> AddOrUpdateAsync(T element)
    {
        try
        {
            if (element == null) return null;
            return element.Id == Guid.Empty ? await AddAsync(element) : await UpdateAsync(element);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Errore nella richiesta: {ex.Message}");
            return null;
        }
    }

    public virtual async Task<bool> UpdateRangeAsync(IEnumerable<T> list)
    {
        try
        {
            using var ctx = _factory.CreateDbContext();
            var dbSet = _dbSetSelector(ctx);
            foreach (var element in list)
            {
                if (element == null) continue;
                var existingItem = await dbSet.FindAsync(element.Id);
                if (existingItem == null) continue;
                EntityUpdater.UpdateEntity(existingItem, element);
                _cache.Remove($"{_cacheKey}_{element.Id}");
            }
            _cache.Remove(_cacheKey);
            await ctx.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Errore nella richiesta: {ex.Message}");
            return false;
        }
    }
}
