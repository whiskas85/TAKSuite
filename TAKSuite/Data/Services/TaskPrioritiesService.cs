using Microsoft.EntityFrameworkCore;
using TAKSuite.Data.Models;


namespace TAKSuite.Data.Services
{
    public class TaskPrioritiesService : DataServiceAbstract<TaskPriority>
    {
        public TaskPrioritiesService(ApplicationDbContext context) : base(context.TaskPriorities, context)
        {
           
        }
    }
}
