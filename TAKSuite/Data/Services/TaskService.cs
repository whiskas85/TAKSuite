using Microsoft.EntityFrameworkCore;
using TAKSuite.Data.Models;

namespace TAKSuite.Data.Services
{
    public class TaskService : DataServiceAbstract<TaskEntity>
    {
        public TaskService(ApplicationDbContext context) : base(context.Tasks, context) { }


        public async Task<TaskEntity> CreateTaskAsync(string name, string description, List<string>? atakPoints = null, List<Guid>? documentIds = null)
        {
            var newTask = new TaskEntity
            {
                Id = Guid.NewGuid(),
                Name = name,
                Description = description,
                Status = TaskStatusTak.Created,
                CreationDate = DateTime.UtcNow,
                LastModified = DateTime.UtcNow,
                Uids = atakPoints ?? new List<string>(),
                Documents = new List<Documentation>()
            };

            // Associa i documenti se presenti
            if (documentIds != null && documentIds.Any())
            {
                var documents = await _context.Documents.Where(d => documentIds.Contains(d.Id)).ToListAsync();
                newTask.Documents.AddRange(documents);
            }

            // Aggiungi il nuovo task al database
            _context.Tasks.Add(newTask);

            // Log della creazione del task
            LogTaskStatusChange(newTask, null, TaskStatusTak.None, TaskStatusTak.Created, "Task created");

            await _context.SaveChangesAsync();
            return newTask;
        }

        public async Task AssignTaskAsync(Guid taskId, Guid teamId)
        {
            var task = await _context.Tasks.FindAsync(taskId) ?? throw new InvalidOperationException("Task not found");

            if (task.Status != TaskStatusTak.Created)
                throw new InvalidOperationException("Task is not in a valid state for assignment");

            LogTaskStatusChange(task, teamId, task.Status, TaskStatusTak.Assigned, "Task assigned to team");
            task.AssignedTeamId = teamId;
            task.Status = TaskStatusTak.Assigned;
            task.AssignedDate = DateTime.UtcNow;
            task.LastModified = DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }

        public async Task AcceptTaskAsync(Guid taskId, Guid teamId)
        {
            var task = await _context.Tasks.FindAsync(taskId) ?? throw new InvalidOperationException("Task not found");

            if (task.AssignedTeamId != teamId)
                throw new UnauthorizedAccessException("Only the assigned team can accept this task");

            if (task.Status != TaskStatusTak.Assigned)
                throw new InvalidOperationException("Task is not in a valid state for acceptance");

            LogTaskStatusChange(task, teamId, task.Status, TaskStatusTak.Accepted, "Task accepted by assigned team");
            task.Status = TaskStatusTak.Accepted;
            task.LastModified = DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }

        public async Task CompleteTaskAsync(Guid taskId, Guid teamId)
        {
            var task = await _context.Tasks.FindAsync(taskId) ?? throw new InvalidOperationException("Task not found");

            if (task.ExecutingTeamId != teamId)
                throw new UnauthorizedAccessException("Only the executing team can complete this task");

            if (task.Status != TaskStatusTak.InProgress)
                throw new InvalidOperationException("Task is not in a valid state for completion");

            LogTaskStatusChange(task, teamId, task.Status, TaskStatusTak.Completed, "Task completed");
            task.Status = TaskStatusTak.Completed;
            task.CompletedDate = DateTime.UtcNow;
            task.LastModified = DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }

        private void LogTaskStatusChange(TaskEntity task, Guid? teamId, TaskStatusTak previous, TaskStatusTak next, string description)
        {
            var log = new TaskLog
            {
                TaskId = task.Id,
                TeamId = teamId,
                PreviousStatus = previous,
                NewStatus = next,
                ActionDescription = description,
                Timestamp = DateTime.UtcNow
            };

            _context.TaskLogs.Add(log);
            task.Logs.Add(log);
        }
    }
}
