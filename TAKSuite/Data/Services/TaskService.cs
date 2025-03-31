using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using TAKSuite.Components.Pages;
using TAKSuite.Data.Models;

namespace TAKSuite.Data.Services
{
    public class TaskService : DataServiceAbstract<TaskEntity>
    {
        public TaskService(ApplicationDbContext context) : base(context.Tasks, context)
        {
            Includes = [ _ =>_.Priority,
                         _=> _.Logs];
        }
       


        public async Task<TaskEntity> InsertTaskByTeamAsync(TaskEntity newTask, Team team, UserAtak user)
        {
            newTask.Status = TaskStatusTak.Accepted;

            // Aggiungi il nuovo task al database
            _context.Tasks.Add(newTask);

            // Log della creazione del task
            LogTaskStatusChange(newTask, team, user, TaskStatusTak.None, TaskStatusTak.Accepted, "Task created by TEAM");

            await _context.SaveChangesAsync();
            return newTask;
        }
        public async Task<TaskEntity> InsertTaskAsync(TaskEntity newTask, UserAtak user)
        {
            newTask.Status = TaskStatusTak.Created;

            // Aggiungi il nuovo task al database
            _context.Tasks.Add(newTask);

            // Log della creazione del task
            LogTaskStatusChange(newTask, null, user, TaskStatusTak.None, TaskStatusTak.Created, "Task created");

            await _context.SaveChangesAsync();
            return newTask;
        }

        //public async Task RejoinTaskTier0Async(TaskEntity task, Team? tier0Team)
        //{
        //    if (task.Status != TaskStatusTak.RejectedTier1)
        //        throw new InvalidOperationException("Task is not in a valid state for assignment");

        //    var newStatus = TaskStatusTak.Created;
        //    LogTaskStatusChange(task, tier0Team, task.Status, newStatus, "Task rejoined from tier0");
        //    task.Status = newStatus;
        //    task.LastModified = DateTime.UtcNow;

        //    await _context.SaveChangesAsync();
        //}



        public async Task ScheduleTaskAsync(TaskEntity task, Team team, UserAtak user)
        {
            if (task.Status != TaskStatusTak.Created)
                throw new InvalidOperationException("Task is not in a valid state for assignment");

            var newStatus = TaskStatusTak.Scheduled;
            LogTaskStatusChange(task, team, user, task.Status, newStatus, "Task scheduled to team");

            task.AssignedTeamId = team.Id;
            task.Status = newStatus;
            task.LastModified = DateTime.UtcNow;

            if(await CheckTeamAutoacceptAsync(team))
            {
                await AcceptTaskAsync(task, team, user);
                return;
            }

            await _context.SaveChangesAsync();
        }

        private async Task<bool> CheckTeamAutoacceptAsync(Team team)
        {
            return false;
        }

        public async Task AcceptTaskAsync(TaskEntity task, Team team, UserAtak user)
        {
            if (task.Status != TaskStatusTak.Scheduled)
                throw new InvalidOperationException("Task is not in a valid state for assignment");

            var newStatus = TaskStatusTak.Accepted;
            LogTaskStatusChange(task, team, user, task.Status, newStatus, "Task has been accepted");
            task.Status = newStatus;
            task.LastModified = DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }

        
        public async Task AssignTaskAsync(TaskEntity task, Team execTeam, UserAtak user)
        {
            if (task.Status != TaskStatusTak.Accepted)
                throw new InvalidOperationException("Task is not in a valid state for assignment");

            var newStatus = TaskStatusTak.Assigned;
            LogTaskStatusChange(task, execTeam, user, task.Status, newStatus, "Task assigned to team");
            task.ExecutingTeamId = execTeam.Id;
            task.Status = newStatus;
            task.LastModified = DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }



        public async Task RejectTaskAsync(TaskEntity task, Team team, UserAtak user)
        {
            var newStatus = TaskStatusTak.None;

            if (task.Status == TaskStatusTak.Accepted || task.Status == TaskStatusTak.Scheduled || task.Status == TaskStatusTak.RejectedTier2)
            {
                if (task.ExecutingTeamId != team.Id)
                    throw new UnauthorizedAccessException("Only the assigned team can complete this task");

                newStatus = TaskStatusTak.RejectedTier1;
                LogTaskStatusChange(task, team, user, task.Status, newStatus, "Task rejected from assigned team");
                task.Status = newStatus;
                task.LastModified = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                return;
            }
            else if (task.Status != TaskStatusTak.Assigned)
            {
                newStatus = TaskStatusTak.RejectedTier2;
                LogTaskStatusChange(task, team, user, task.Status, newStatus, "Task rejected by the team");
                task.Status = newStatus;
                task.LastModified = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                return;
            }

            throw new InvalidOperationException("Task is not in a valid state for assignment");
        }

        public async Task RejoinTaskAsync(TaskEntity task, Team team, UserAtak user)
        {
           
            if (task.Status == TaskStatusTak.RejectedTier2)
            {
                if (task.ExecutingTeamId != team.Id)
                    throw new UnauthorizedAccessException("Only the assigned team can complete this task");

                var newStatus = TaskStatusTak.Accepted;
                LogTaskStatusChange(task, team, user, task.Status, newStatus, "Task rejoined from assigned team");
                task.Status = newStatus;
                task.LastModified = DateTime.UtcNow;

                await _context.SaveChangesAsync();
            }
            else if (task.Status != TaskStatusTak.RejectedTier1)
            {
                var newStatus = TaskStatusTak.Created;
                LogTaskStatusChange(task, team, user, task.Status, newStatus, "Task rejoined from tier0");
                task.Status = newStatus;
                task.LastModified = DateTime.UtcNow;

                await _context.SaveChangesAsync();
            }

                throw new InvalidOperationException("Task is not in a valid state for assignment");
        }


        //public async Task RejoinTaskTier1Async(TaskEntity task, Team assignedTeam)
        //{
        //    if (task.ExecutingTeamId != assignedTeam.Id)
        //        throw new UnauthorizedAccessException("Only the assigned team can complete this task");

        //    if (task.Status != TaskStatusTak.RejectedTier2)
        //        throw new InvalidOperationException("Task is not in a valid state for assignment");

        //    var newStatus = TaskStatusTak.Accepted;
        //    LogTaskStatusChange(task, assignedTeam, task.Status, newStatus, "Task rejoined from assigned team");
        //    task.Status = newStatus;
        //    task.LastModified = DateTime.UtcNow;

        //    await _context.SaveChangesAsync();
        //}

        // TASK Execution Logic
        public async Task ExecutingTaskAsync(TaskEntity task, Team execTeam, UserAtak user)
        {
            if (task.ExecutingTeamId != execTeam.Id)
                throw new UnauthorizedAccessException("Only the executing team can complete this task");

            if (task.Status != TaskStatusTak.Assigned)
                throw new InvalidOperationException("Task is not in a valid state for assignment");

            var newStatus = TaskStatusTak.InProgress;
            LogTaskStatusChange(task, execTeam, user, task.Status, newStatus, "Task start it's execution");
            task.Status = newStatus;
            task.LastModified = DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }

        // TASK Ending logic
        public async Task CompleteTaskAsync(TaskEntity task, Team execTeam, UserAtak user)
        {
            if (task.ExecutingTeamId != execTeam.Id)
                throw new UnauthorizedAccessException("Only the executing team can complete this task");

            if (task.Status != TaskStatusTak.InProgress)
                throw new InvalidOperationException("Task is not in a valid state for completion");

            var newStatus = TaskStatusTak.Completed;
            LogTaskStatusChange(task, execTeam, user, task.Status, newStatus, "Task completed");
            task.Status = newStatus;
            task.LastModified = DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }

        public async Task AbortTaskAsync(TaskEntity task, Team execTeam, UserAtak user)
        {
            if (task.ExecutingTeamId != execTeam.Id)
                throw new UnauthorizedAccessException("Only the executing team can complete this task");

            if (task.Status != TaskStatusTak.InProgress)
                throw new InvalidOperationException("Task is not in a valid state for completion");

            var newStatus = TaskStatusTak.Aborted;
            LogTaskStatusChange(task, execTeam, user, task.Status, newStatus, "Task aborted by team");
            task.Status = newStatus;

            task.LastModified = DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }
        public async Task FailedTaskAsync(TaskEntity task, Team execTeam, UserAtak user)
        {
            if (task.ExecutingTeamId != execTeam.Id)
                throw new UnauthorizedAccessException("Only the executing team can complete this task");

            if (task.Status != TaskStatusTak.InProgress)
                throw new InvalidOperationException("Task is not in a valid state for completion");

            var newStatus = TaskStatusTak.Failed;
            LogTaskStatusChange(task, execTeam, user, task.Status, newStatus, "Task failed by team");
            task.Status = newStatus;

            task.LastModified = DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }




        // Task Cancellation
        public async Task CancelTaskAsync(TaskEntity task, Team cancelTeam, UserAtak user)
        {
            var newStatus = TaskStatusTak.Canceled;
            LogTaskStatusChange(task, cancelTeam, user, task.Status, newStatus, "Task cancelled");
            task.Status = newStatus;

            task.LastModified = DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }



        private void LogTaskStatusChange(TaskEntity task, Team? team, UserAtak user, TaskStatusTak previous, TaskStatusTak next, string description)
        {
            var log = new TaskLog
            {
                UserId = user.Id,
                TaskId = task.Id,
                TeamId = team?.Id,
                PreviousStatus = previous,
                NewStatus = next,
                ActionDescription = description,
                Timestamp = DateTime.UtcNow
            };

            _context.TaskLogs.Add(log);
            task.Logs.Add(log);
        }

        public async Task<List<TaskEntity>> GetAllTaskAssignedToTeamAsync(Guid teamId)
        {
            try
            {
                var query = DBSet as IQueryable<TaskEntity>;

                if (Includes != null)
                {
                    foreach (var include in Includes)
                    {
                        query = query.Include(include);
                    }
                }
                return await query.Where(_ => (_.AssignedTeam != null && _.AssignedTeam.Id == teamId) || 
                                              (_.ExecutingTeam!= null && _.ExecutingTeam.Id ==teamId))
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Errore nella richiesta: {ex.Message}");
                return new();

            }
        }
        public async Task<List<TaskEntity>> GetAllTaskAssignedToTeamAsync(Team team)
        {
            return await GetAllTaskAssignedToTeamAsync(team.Id);
        }
    }
}
