using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using TAKSuite.Data.Models;
using TAKSuite.Data.Services.BaseDataManagement;


namespace TAKSuite.Data.Services
{
    public class TaskPrioritiesService : DataServiceAbstract<TaskPriority>, IDataProvider
    {
        public TaskPrioritiesService(ApplicationDbContext context, IMemoryCache cache) : base(context.TaskPriorities, context, cache)
        {
           
        }

        public Type ProvidedItem => typeof(TaskPriority);
    }
}
