using TAKSuite.Data.Models;

namespace TAKSuite.Data.Services.BaseDataManagement
{
    public interface IDataService<T> where T : class, new()
    {
        Task<T> AddAsync(T element);
        Task<bool> DeleteAsync(Guid id);
        Task<List<T>> GetAllAsync();
        Task<T?> GetAsync(Guid id);
        Task<T> UpdateAsync(T element);
    }
}