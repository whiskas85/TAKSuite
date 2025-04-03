using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using TAKSuite.Data.Models;


namespace TAKSuite.Data.Services
{
    public class TaskPrioritiesService : DataServiceAbstract<TaskPriority>
    {
        public TaskPrioritiesService(ApplicationDbContext context, IMemoryCache cache) : base(context.TaskPriorities, context, cache)
        {
           
        }
    }
}
