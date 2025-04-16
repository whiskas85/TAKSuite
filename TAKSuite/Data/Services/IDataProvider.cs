using TAKSuite.Data.Models;

namespace TAKSuite.Data.Services
{
    public interface IDataProvider
    {
        Type ProvidedItem { get; }
    }
}