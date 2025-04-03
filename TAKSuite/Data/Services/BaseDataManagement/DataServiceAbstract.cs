using TAKSuite.Data.Models;
using TAKSuite.Data.Services.BaseDataManagement;
using TAKSuite.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using Microsoft.AspNetCore.Mvc;
using BlazorBootstrap;
using Microsoft.Extensions.Caching.Memory;
using System.Xml.Linq;

public abstract class DataServiceAbstract<T> : IService, IDataService<T> where T : class, IGuidModel, new()
{
    protected DbSet<T> DBSet { get; set; }
    internal readonly ApplicationDbContext _context;
    public Expression<Func<T, object>>[]? Includes { get; set; }
    
    
    private readonly IMemoryCache _cache;
    private readonly string _cacheKey = typeof(T).Name;

    protected DataServiceAbstract(DbSet<T> dbSet, ApplicationDbContext context, IMemoryCache cache)
    {
        DBSet = dbSet;
        _context = context;
        _cache = cache;
    }
    public virtual T CreateNew()
    {
        T element = new T();
        return element;
    }


    public virtual async Task<T> AddAsync(T element)
    {
        try
        {
            if (element == null) return null;

            element.Id = Guid.NewGuid();
            DBSet.Add(element);
            await _context.SaveChangesAsync();
            _cache.Remove(_cacheKey);  // Invalida la cache dopo un'aggiunta
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
            var item = await DBSet.FindAsync(id);
            if (item == null) return false;

            DBSet.Remove(item);
            await _context.SaveChangesAsync();
            _cache.Remove(_cacheKey); // Invalida la cache dopo una cancellazione
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
            //if (_cache.TryGetValue(_cacheKey, out List<T> cachedData))
            //{
            //    return cachedData;
            //}

            IQueryable<T> query = DBSet;
            if (Includes != null)
            {
                foreach (var include in Includes)
                {
                    query = query.Include(include);
                }
            }

            var data = await query.ToListAsync();
            _cache.Set(_cacheKey, data, TimeSpan.FromMinutes(10)); // Memorizza la cache per 10 minuti
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
            string cacheItemKey = $"{_cacheKey}_{id}";

            //if (_cache.TryGetValue(cacheItemKey, out T cachedItem))
            //{
            //    return cachedItem;
            //}

            IQueryable<T> query = DBSet;
            if (Includes != null)
            {
                foreach (var include in Includes)
                {
                    query = query.Include(include);
                }
            }

            var item = await query.FirstOrDefaultAsync(r => r.Id == id);
            if (item != null)
            {
                _cache.Set(cacheItemKey, item, TimeSpan.FromMinutes(10));
            }

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

            var existingItem = await DBSet.FindAsync(element.Id);
            if (existingItem == null) return null;

            EntityUpdater.UpdateEntity(existingItem, element);
            await _context.SaveChangesAsync();

            _cache.Remove(_cacheKey); // Invalida la cache dopo un aggiornamento
            _cache.Remove($"{_cacheKey}_{element.Id}");

            return element;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Errore nella richiesta: {ex.Message}");
            return null;
        }
    }









    //public virtual async Task<T> AddAsync(T element)
    //{
    //    try
    //    {
    //        if (element == null) return null;

    //        element.Id = Guid.NewGuid();
    //        DBSet.Add(element);
    //        await _context.SaveChangesAsync();
    //        return element;
    //    }
    //    catch (Exception ex)
    //    {
    //        Console.WriteLine($"Errore nella richiesta: {ex.Message}");
    //        return null;
    //    }
    //}
    public virtual async Task<T> AddOrUpdateAsync(T element)
    {
        try
        {
            if (element == null) return null;

            if (element.Id == Guid.Empty)
            {
                return await AddAsync(element);
            }
            else
            {
                return await UpdateAsync(element);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Errore nella richiesta: {ex.Message}");
            return null;
        }
    }

    //public virtual async Task<bool> DeleteAsync(Guid id)
    //{
    //    try
    //    {
    //        var item = await DBSet.FindAsync(id);
    //        if (item == null) return false;

    //        DBSet.Remove(item);
    //        await _context.SaveChangesAsync();
    //        return true;
    //    }
    //    catch (Exception ex)
    //    {
    //        Console.WriteLine($"Errore nella richiesta: {ex.Message}");
    //        return false;
    //    }
    //}

    //public virtual async Task<List<T>> GetAllAsync()
    //{
    //    try
    //    {
    //        IQueryable<T> query = DBSet;

    //        if (Includes != null)
    //        {
    //            foreach (var include in Includes)
    //            {
    //                query = query.Include(include);
    //            }
    //        }

    //        return await query.ToListAsync();
    //    }
    //    catch (Exception ex)
    //    {
    //        Console.WriteLine($"Errore nella richiesta: {ex.Message}");
    //        return new List<T>();
            
    //    }
    //}

    //public virtual async Task<T?> GetAsync(Guid id)
    //{
    //    try
    //    {
    //        IQueryable<T> query = DBSet;

    //        if (Includes != null)
    //        {
    //            foreach (var include in Includes)
    //            {
    //                query = query.Include(include);
    //            }
    //        }

    //        return await query.FirstOrDefaultAsync(r => r.Id == id);
    //    }
    //    catch (Exception ex)
    //    {
    //        Console.WriteLine($"Errore nella richiesta: {ex.Message}");
    //        return null;
    //    }
    //}

    //public virtual async Task<T> UpdateAsync(T element)
    //{
    //    try
    //    {
    //        if (element == null) return null;

    //        var existingItem = await DBSet.FindAsync(element.Id);
    //        if (existingItem == null) return null;

    //        EntityUpdater.UpdateEntity(existingItem, element);
    //        await _context.SaveChangesAsync();
    //        return element;
    //    }
    //    catch (Exception ex)
    //    {
    //        Console.WriteLine($"Errore nella richiesta: {ex.Message}");
    //        return null;
    //    }
    //}

    public virtual async Task<bool> UpdateRangeAsync(IEnumerable<T> list)
    {
        try
        {
            list.ToList().ForEach(async element =>
            {
                if (element == null) return;

                var existingItem = await DBSet.FindAsync(element.Id);
                if (existingItem == null) return;

                EntityUpdater.UpdateEntity(existingItem, element);
                
                _cache.Remove($"{_cacheKey}_{element.Id}");
            });

            _cache.Remove(_cacheKey); // Invalida la cache dopo un aggiornamento

            await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Errore nella richiesta: {ex.Message}");
            return false;
        }
    }

}
