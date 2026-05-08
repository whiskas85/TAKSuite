using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Org.BouncyCastle.Tsp;
using System.Threading.Tasks;
using TAKSuite.Components.Pages;
using TAKSuite.Data.Models;
using TAKSuite.Data.Services.BaseDataManagement;

namespace TAKSuite.Data.Services
{
    public class TaskService : DataServiceAbstract<TaskEntity>
    {
        public TaskService(ApplicationDbContext context, IMemoryCache cache) : base(context.Tasks, context, cache)
        {
            Includes = [ _ =>_.Priority,
                         _=> _.Logs,
                         _=> _.Hierarchy,
                         _=> _.Items,
                         _=> _.RadioChannel,
                         _=> _.AssignedTeam,
                         _=> _.ExecutingTeam];
        }
       


        public override async Task<TaskEntity> AddAsync(TaskEntity element)
        {
            if (element == null) return null;

            element.Status = TaskStatusTak.Created;

            if (element.MissionTAKSuiteId.HasValue)
            {
                var mission = await _context.MissionsTakSuite.FindAsync(element.MissionTAKSuiteId.Value);
                if (mission != null)
                {
                    element.StartDateTime ??= mission.StartDateTime;
                    element.EndDateTime ??= mission.EndDateTime;

                    if (element.AssignedTeamId.HasValue && mission.AutoScheduleTeam)
                    {
                        element.Status = TaskStatusTak.Scheduled;

                        if (mission.AutoAssignTeam)
                        {
                            element.Status = TaskStatusTak.Accepted;

                            if (mission.AutoAcceptTask)
                            {
                                element.ExecutingTeamId ??= element.AssignedTeamId;
                                element.Status = TaskStatusTak.Assigned;
                            }
                        }
                    }
                }
            }

            return await base.AddAsync(element);
        }

        public override async Task<TaskEntity> UpdateAsync(TaskEntity element)
        {
            if (element == null) return null;

            var taskId = element.Id;
            var incomingItems = element.Items
                .Select(i => new { i.Id, i.Value, i.Type, i.Order })
                .ToList();

            // Blazor Server: il DbContext è scoped al circuito, puliamo il tracker
            _context.ChangeTracker.Clear();

            // --- ITEMS: gestione esplicita tramite DbSet, senza navigation property ---

            var dbItems = await _context.TaskStringItems
                .Where(i => i.TaskEntityId == taskId)
                .ToListAsync();

            // Elimina items rimossi
            var toDelete = dbItems
                .Where(db => !incomingItems.Any(i => i.Id == db.Id))
                .ToList();
            _context.TaskStringItems.RemoveRange(toDelete);

            // Aggiorna esistenti / inserisci nuovi
            foreach (var incoming in incomingItems)
            {
                var existing = dbItems.FirstOrDefault(db => db.Id == incoming.Id);
                if (existing != null)
                {
                    existing.Value = incoming.Value;
                    existing.Type = incoming.Type;
                    existing.Order = incoming.Order;
                }
                else
                {
                    _context.TaskStringItems.Add(new TaskStringItem
                    {
                        Value = incoming.Value,
                        Type = incoming.Type,
                        Order = incoming.Order,
                        TaskEntityId = taskId
                    });
                }
            }

            // --- TASK: aggiorna solo le proprietà scalari ---

            var existingTask = await DBSet.FindAsync(taskId);
            if (existingTask == null) return null;

            EntityUpdater.UpdateEntity(existingTask, element);

            await _context.SaveChangesAsync();

            _cache.Remove(typeof(TaskEntity).Name);
            _cache.Remove($"{typeof(TaskEntity).Name}_{taskId}");

            return element;
        }

        public async Task<TaskEntity> InsertTaskByTeamAsync(TaskEntity newTask, Team team, UserAtak user)
        {
            newTask.Status = TaskStatusTak.Scheduled;

            // Aggiungi il nuovo task al database
            _context.Tasks.Add(newTask);

            // push hierarchy
            //PushTaskScheduled(newTask, team);

            // Log della creazione del task
            LogTaskStatusChange(newTask, team, user, TaskStatusTak.None, TaskStatusTak.Created, "Il task è stato creato dal team");
            LogTaskStatusChange(newTask, team, user, TaskStatusTak.Created, TaskStatusTak.Scheduled, "Il task è stato schedulato al team");

            await _context.SaveChangesAsync();

            return newTask;
        }
        public async Task<TaskEntity> InsertTaskAsync(TaskEntity newTask, UserAtak user)
        {
            newTask.Status = TaskStatusTak.Created;

            // Aggiungi il nuovo task al database
            _context.Tasks.Add(newTask);

            // Log della creazione del task
            LogTaskStatusChange(newTask, null, user, TaskStatusTak.None, TaskStatusTak.Created, "Il task è stato creato");

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
            if (task.Status != TaskStatusTak.Created && task.Status != TaskStatusTak.Scheduled)
                throw new InvalidOperationException("Task is not in a valid state for assignment");


            var newStatus = TaskStatusTak.Scheduled;
            
            if(task.Status== TaskStatusTak.Created) 
                LogTaskStatusChange(task, team, user, task.Status, newStatus, $"Il task è stato schedulato al team");
            else 
                LogTaskStatusChange(task, team, user, task.Status, newStatus, $"Il task è stato rischedulato da {task.AssignedTeam.Name} -> {team.Name}");
            
            
            if (task.AssignedTeam!= null)
                PushTaskScheduled(task, task.AssignedTeam);          
            
            task.AssignedTeamId = team.Id;
            task.Status = newStatus;
            task.LastModified = DateTime.UtcNow;

            // push hierarchy
            


            if (await CheckTeamAutoacceptAsync(team))
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
            if (task.Status == TaskStatusTak.Accepted || task.Status == TaskStatusTak.Scheduled || task.Status == TaskStatusTak.RejectedTier2)
            {
                if (task.AssignedTeamId != team.Id)
                    throw new UnauthorizedAccessException("Only the assigned team can complete this task");

                if (HasHierarchyHops(task))
                {
                    // status remain the same
                    var newStatus = task.Status;
                    task.Status = newStatus;


                    // get the last scheduled team hop
                    var teamHop = PopTaskScheduled(task);
                    task.AssignedTeamId = teamHop.Id;
                    
                    task.LastModified = DateTime.UtcNow;
                    
                    LogTaskStatusChange(task, teamHop, user, task.Status, newStatus, "Task rejected from assigned team");
                }
                else
                {
                    var newStatus = TaskStatusTak.RejectedTier1;
                    LogTaskStatusChange(task, team, user, task.Status, newStatus, "Task rejected from assigned team");
                    task.Status = newStatus;
                    task.LastModified = DateTime.UtcNow;
                }

                await _context.SaveChangesAsync();
                return;
            }
            else if (task.Status == TaskStatusTak.Assigned)
            {
                if (task.ExecutingTeamId != team.Id)
                    throw new UnauthorizedAccessException("Only the executing team can complete this task");

                var newStatus = TaskStatusTak.RejectedTier2;
                LogTaskStatusChange(task, team, user, task.Status, newStatus, "Il task è stato rifiutato dal team di esecuzione");
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
                if (task.AssignedTeamId != team.Id)
                    throw new UnauthorizedAccessException("Only the assigned team can complete this task");

                var newStatus = TaskStatusTak.Accepted;
                LogTaskStatusChange(task, team, user, task.Status, newStatus, "Task rejoined from assigned team");
                task.Status = newStatus;
                task.LastModified = DateTime.UtcNow;

                await _context.SaveChangesAsync();
            }
            else if (task.Status == TaskStatusTak.RejectedTier1)
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


        private void PushTaskScheduled(TaskEntity task, Team team)
        {
            var hierarchy = new TaskHierarchy
            {
                TaskId = task.Id,
                TeamId = team.Id,
                Timestamp = DateTime.UtcNow
            };
            _context.TaskHierarchy.Add(hierarchy);
            task.Hierarchy.Add(hierarchy);
        }
        private bool HasHierarchyHops(TaskEntity task)
        {
            // vado a recuperare tutti i cambi di assigned team
            return _context.TaskHierarchy.Where(_ => _.Task.Id == task.Id)
                .Count() >= 1;
        }
        private Team PopTaskScheduled(TaskEntity task)
        {
            var hierarchyLastHop = _context.TaskHierarchy.Where(_ => _.Task.Id == task.Id)
                .OrderBy(_=> _.Timestamp)
                .ToList()
                .Last();
            _context.TaskHierarchy.Remove(hierarchyLastHop);
            return hierarchyLastHop.Team;
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

                return await query.Where(task =>
                    (task.AssignedTeamId == teamId) ||
                    (task.ExecutingTeamId == teamId) ||
                    task.Hierarchy.Any(h => h.TeamId == teamId) // Verifica se il teamId è presente in TaskHierarchy
                ).ToListAsync();
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
