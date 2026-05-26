using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using TAKSuite.Data.Models;
using TAKSuite.Data.Services.BaseDataManagement;

namespace TAKSuite.Data.Services
{
    public class TaskService : DataServiceAbstract<TaskEntity>
    {
        public TaskService(IDbContextFactory<ApplicationDbContext> factory, IMemoryCache cache) : base(factory, ctx => ctx.Tasks, cache)
        {
            Includes = [_ => _.Priority,
                         _ => _.Logs,
                         _ => _.Hierarchy,
                         _ => _.Items,
                         _ => _.RadioChannel,
                         _ => _.AssignedTeam,
                         _ => _.ExecutingTeam];
        }

        public override async Task<TaskEntity> AddAsync(TaskEntity element)
        {
            if (element == null) return null;
            element.Status = TaskStatusTak.Created;

            if (element.MissionTAKSuiteId.HasValue)
            {
                using var ctx = _factory.CreateDbContext();
                var mission = await ctx.MissionsTakSuite.FindAsync(element.MissionTAKSuiteId.Value);
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

            using var ctx = _factory.CreateDbContext();

            var dbItems = await ctx.TaskStringItems
                .Where(i => i.TaskEntityId == taskId)
                .ToListAsync();

            var toDelete = dbItems
                .Where(db => !incomingItems.Any(i => i.Id == db.Id))
                .ToList();
            ctx.TaskStringItems.RemoveRange(toDelete);

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
                    ctx.TaskStringItems.Add(new TaskStringItem
                    {
                        Value = incoming.Value,
                        Type = incoming.Type,
                        Order = incoming.Order,
                        TaskEntityId = taskId
                    });
                }
            }

            var existingTask = await ctx.Tasks.FindAsync(taskId);
            if (existingTask == null) return null;

            EntityUpdater.UpdateEntity(existingTask, element);
            await ctx.SaveChangesAsync();

            _cache.Remove(typeof(TaskEntity).Name);
            _cache.Remove($"{typeof(TaskEntity).Name}_{taskId}");

            return element;
        }

        public async Task<TaskEntity> InsertTaskByTeamAsync(TaskEntity newTask, Team team, UserAtak user)
        {
            newTask.Status = TaskStatusTak.Scheduled;
            if (newTask.Id == Guid.Empty) newTask.Id = Guid.NewGuid();

            using var ctx = _factory.CreateDbContext();
            ctx.Tasks.Add(newTask);
            LogTaskStatusChange(ctx, newTask, team, user, TaskStatusTak.None, TaskStatusTak.Created, "Il task è stato creato dal team");
            LogTaskStatusChange(ctx, newTask, team, user, TaskStatusTak.Created, TaskStatusTak.Scheduled, "Il task è stato schedulato al team");
            await ctx.SaveChangesAsync();
            return newTask;
        }

        public async Task<TaskEntity> InsertTaskAsync(TaskEntity newTask, UserAtak user)
        {
            newTask.Status = TaskStatusTak.Created;
            if (newTask.Id == Guid.Empty) newTask.Id = Guid.NewGuid();

            using var ctx = _factory.CreateDbContext();
            ctx.Tasks.Add(newTask);
            LogTaskStatusChange(ctx, newTask, null, user, TaskStatusTak.None, TaskStatusTak.Created, "Il task è stato creato");
            await ctx.SaveChangesAsync();
            return newTask;
        }

        public async Task ScheduleTaskAsync(TaskEntity task, Team team, UserAtak user)
        {
            using var ctx = _factory.CreateDbContext();
            var fresh = await ctx.Tasks.Include(t => t.AssignedTeam).FirstOrDefaultAsync(t => t.Id == task.Id);
            if (fresh == null) throw new InvalidOperationException("Task not found");

            if (fresh.Status != TaskStatusTak.Created && fresh.Status != TaskStatusTak.Scheduled)
                throw new InvalidOperationException("Task is not in a valid state for assignment");

            var newStatus = TaskStatusTak.Scheduled;

            if (fresh.Status == TaskStatusTak.Created)
                LogTaskStatusChange(ctx, fresh, team, user, fresh.Status, newStatus, $"Il task è stato schedulato al team");
            else
                LogTaskStatusChange(ctx, fresh, team, user, fresh.Status, newStatus, $"Il task è stato rischedulato da {fresh.AssignedTeam?.Name} -> {team.Name}");

            if (fresh.AssignedTeam != null)
                PushTaskScheduled(ctx, fresh, fresh.AssignedTeam);

            fresh.AssignedTeamId = team.Id;
            fresh.Status = newStatus;
            fresh.LastModified = DateTime.UtcNow;

            if (await CheckTeamAutoacceptAsync(team))
            {
                await AcceptTaskCoreAsync(ctx, fresh, team, user);
            }

            await ctx.SaveChangesAsync();
        }

        public async Task AcceptTaskAsync(TaskEntity task, Team team, UserAtak user)
        {
            using var ctx = _factory.CreateDbContext();
            var fresh = await ctx.Tasks.FindAsync(task.Id);
            if (fresh == null) return;
            await AcceptTaskCoreAsync(ctx, fresh, team, user);
            await ctx.SaveChangesAsync();
        }

        private async Task AcceptTaskCoreAsync(ApplicationDbContext ctx, TaskEntity task, Team team, UserAtak user)
        {
            if (task.Status != TaskStatusTak.Scheduled)
                throw new InvalidOperationException("Task is not in a valid state for assignment");

            var newStatus = TaskStatusTak.Accepted;
            LogTaskStatusChange(ctx, task, team, user, task.Status, newStatus, "Task has been accepted");
            task.Status = newStatus;
            task.LastModified = DateTime.UtcNow;
        }

        public async Task AssignTaskAsync(TaskEntity task, Team execTeam, UserAtak user)
        {
            using var ctx = _factory.CreateDbContext();
            var fresh = await ctx.Tasks.FindAsync(task.Id);
            if (fresh == null) return;

            if (fresh.Status != TaskStatusTak.Accepted)
                throw new InvalidOperationException("Task is not in a valid state for assignment");

            var newStatus = TaskStatusTak.Assigned;
            LogTaskStatusChange(ctx, fresh, execTeam, user, fresh.Status, newStatus, "Task assigned to team");
            fresh.ExecutingTeamId = execTeam.Id;
            fresh.Status = newStatus;
            fresh.LastModified = DateTime.UtcNow;
            await ctx.SaveChangesAsync();
        }

        public async Task RejectTaskAsync(TaskEntity task, Team team, UserAtak user)
        {
            using var ctx = _factory.CreateDbContext();
            var fresh = await ctx.Tasks.Include(t => t.Hierarchy).FirstOrDefaultAsync(t => t.Id == task.Id);
            if (fresh == null) return;

            if (fresh.Status == TaskStatusTak.Accepted || fresh.Status == TaskStatusTak.Scheduled || fresh.Status == TaskStatusTak.RejectedTier2)
            {
                if (fresh.AssignedTeamId != team.Id)
                    throw new UnauthorizedAccessException("Only the assigned team can complete this task");

                if (HasHierarchyHops(ctx, fresh))
                {
                    var newStatus = fresh.Status;
                    fresh.Status = newStatus;
                    var teamHop = PopTaskScheduled(ctx, fresh);
                    fresh.AssignedTeamId = teamHop.Id;
                    fresh.LastModified = DateTime.UtcNow;
                    LogTaskStatusChange(ctx, fresh, teamHop, user, fresh.Status, newStatus, "Task rejected from assigned team");
                }
                else
                {
                    var newStatus = TaskStatusTak.RejectedTier1;
                    LogTaskStatusChange(ctx, fresh, team, user, fresh.Status, newStatus, "Task rejected from assigned team");
                    fresh.Status = newStatus;
                    fresh.LastModified = DateTime.UtcNow;
                }

                await ctx.SaveChangesAsync();
                return;
            }

            if (fresh.Status == TaskStatusTak.Assigned)
            {
                if (fresh.ExecutingTeamId != team.Id)
                    throw new UnauthorizedAccessException("Only the executing team can complete this task");

                var newStatus = TaskStatusTak.RejectedTier2;
                LogTaskStatusChange(ctx, fresh, team, user, fresh.Status, newStatus, "Il task è stato rifiutato dal team di esecuzione");
                fresh.Status = newStatus;
                fresh.LastModified = DateTime.UtcNow;
                await ctx.SaveChangesAsync();
                return;
            }

            throw new InvalidOperationException("Task is not in a valid state for assignment");
        }

        public async Task RejoinTaskAsync(TaskEntity task, Team team, UserAtak user)
        {
            using var ctx = _factory.CreateDbContext();
            var fresh = await ctx.Tasks.FindAsync(task.Id);
            if (fresh == null) return;

            if (fresh.Status == TaskStatusTak.RejectedTier2)
            {
                if (fresh.AssignedTeamId != team.Id)
                    throw new UnauthorizedAccessException("Only the assigned team can complete this task");

                var newStatus = TaskStatusTak.Accepted;
                LogTaskStatusChange(ctx, fresh, team, user, fresh.Status, newStatus, "Task rejoined from assigned team");
                fresh.Status = newStatus;
                fresh.LastModified = DateTime.UtcNow;
                await ctx.SaveChangesAsync();
                return;
            }

            if (fresh.Status == TaskStatusTak.RejectedTier1)
            {
                var newStatus = TaskStatusTak.Created;
                LogTaskStatusChange(ctx, fresh, team, user, fresh.Status, newStatus, "Task rejoined from tier0");
                fresh.Status = newStatus;
                fresh.LastModified = DateTime.UtcNow;
                await ctx.SaveChangesAsync();
                return;
            }

            throw new InvalidOperationException("Task is not in a valid state for assignment");
        }

        public async Task ExecutingTaskAsync(TaskEntity task, Team execTeam, UserAtak user)
        {
            using var ctx = _factory.CreateDbContext();
            var fresh = await ctx.Tasks.FindAsync(task.Id);
            if (fresh == null) return;

            if (fresh.ExecutingTeamId != execTeam.Id)
                throw new UnauthorizedAccessException("Only the executing team can complete this task");
            if (fresh.Status != TaskStatusTak.Assigned)
                throw new InvalidOperationException("Task is not in a valid state for assignment");

            var newStatus = TaskStatusTak.InProgress;
            LogTaskStatusChange(ctx, fresh, execTeam, user, fresh.Status, newStatus, "Task start it's execution");
            fresh.Status = newStatus;
            fresh.LastModified = DateTime.UtcNow;
            await ctx.SaveChangesAsync();
        }

        public async Task CompleteTaskAsync(TaskEntity task, Team execTeam, UserAtak user)
        {
            using var ctx = _factory.CreateDbContext();
            var fresh = await ctx.Tasks.FindAsync(task.Id);
            if (fresh == null) return;

            if (fresh.ExecutingTeamId != execTeam.Id)
                throw new UnauthorizedAccessException("Only the executing team can complete this task");
            if (fresh.Status != TaskStatusTak.InProgress)
                throw new InvalidOperationException("Task is not in a valid state for completion");

            var newStatus = TaskStatusTak.Completed;
            LogTaskStatusChange(ctx, fresh, execTeam, user, fresh.Status, newStatus, "Task completed");
            fresh.Status = newStatus;
            fresh.LastModified = DateTime.UtcNow;
            await ctx.SaveChangesAsync();
        }

        public async Task AbortTaskAsync(TaskEntity task, Team execTeam, UserAtak user)
        {
            using var ctx = _factory.CreateDbContext();
            var fresh = await ctx.Tasks.FindAsync(task.Id);
            if (fresh == null) return;

            if (fresh.ExecutingTeamId != execTeam.Id)
                throw new UnauthorizedAccessException("Only the executing team can complete this task");
            if (fresh.Status != TaskStatusTak.InProgress)
                throw new InvalidOperationException("Task is not in a valid state for completion");

            var newStatus = TaskStatusTak.Aborted;
            LogTaskStatusChange(ctx, fresh, execTeam, user, fresh.Status, newStatus, "Task aborted by team");
            fresh.Status = newStatus;
            fresh.LastModified = DateTime.UtcNow;
            await ctx.SaveChangesAsync();
        }

        public async Task FailedTaskAsync(TaskEntity task, Team execTeam, UserAtak user)
        {
            using var ctx = _factory.CreateDbContext();
            var fresh = await ctx.Tasks.FindAsync(task.Id);
            if (fresh == null) return;

            if (fresh.ExecutingTeamId != execTeam.Id)
                throw new UnauthorizedAccessException("Only the executing team can complete this task");
            if (fresh.Status != TaskStatusTak.InProgress)
                throw new InvalidOperationException("Task is not in a valid state for completion");

            var newStatus = TaskStatusTak.Failed;
            LogTaskStatusChange(ctx, fresh, execTeam, user, fresh.Status, newStatus, "Task failed by team");
            fresh.Status = newStatus;
            fresh.LastModified = DateTime.UtcNow;
            await ctx.SaveChangesAsync();
        }

        public async Task CancelTaskAsync(TaskEntity task, Team cancelTeam, UserAtak user)
        {
            using var ctx = _factory.CreateDbContext();
            var fresh = await ctx.Tasks.FindAsync(task.Id);
            if (fresh == null) return;

            var newStatus = TaskStatusTak.Canceled;
            LogTaskStatusChange(ctx, fresh, cancelTeam, user, fresh.Status, newStatus, "Task cancelled");
            fresh.Status = newStatus;
            fresh.LastModified = DateTime.UtcNow;
            await ctx.SaveChangesAsync();
        }

        public async Task<List<TaskEntity>> GetAllTaskAssignedToTeamAsync(Guid teamId)
        {
            try
            {
                using var ctx = _factory.CreateDbContext();
                IQueryable<TaskEntity> query = ctx.Tasks;
                if (Includes != null)
                {
                    foreach (var include in Includes)
                        query = query.Include(include);
                }
                return await query.Where(task =>
                    task.AssignedTeamId == teamId ||
                    task.ExecutingTeamId == teamId ||
                    task.Hierarchy.Any(h => h.TeamId == teamId)
                ).ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Errore nella richiesta: {ex.Message}");
                return new();
            }
        }

        public async Task<List<TaskEntity>> GetAllTaskAssignedToTeamAsync(Team team) =>
            await GetAllTaskAssignedToTeamAsync(team.Id);

        public async Task SetGreenLightAsync(Guid taskId, DateTime greenLightUtc)
        {
            using var ctx = _factory.CreateDbContext();
            var task = await ctx.Tasks.FindAsync(taskId);
            if (task == null) return;
            task.GreenLightDateTime = greenLightUtc;
            task.LastModified = DateTime.UtcNow;
            await ctx.SaveChangesAsync();
            _cache.Remove(typeof(TaskEntity).Name);
            _cache.Remove($"{typeof(TaskEntity).Name}_{taskId}");
        }

        public async Task UpdateTaskDatesAsync(Guid taskId, DateTime? startUtc, DateTime? endUtc)
        {
            using var ctx = _factory.CreateDbContext();
            var task = await ctx.Tasks.FindAsync(taskId);
            if (task == null) return;
            task.StartDateTime = startUtc;
            task.EndDateTime   = endUtc;
            task.LastModified  = DateTime.UtcNow;
            await ctx.SaveChangesAsync();
            _cache.Remove(typeof(TaskEntity).Name);
            _cache.Remove($"{typeof(TaskEntity).Name}_{taskId}");
        }

        public async Task<List<TaskTimeWindow>> GetTaskTimeWindowsAsync(Guid taskId)
        {
            using var ctx = _factory.CreateDbContext();
            var task = await ctx.Tasks.FindAsync(taskId);
            if (task?.TimeWindowsJson == null) return new();
            try { return System.Text.Json.JsonSerializer.Deserialize<List<TaskTimeWindow>>(task.TimeWindowsJson) ?? new(); }
            catch { return new(); }
        }

        public async Task UpdateTaskTimeWindowsAsync(Guid taskId, List<TaskTimeWindow> windows)
        {
            using var ctx = _factory.CreateDbContext();
            var task = await ctx.Tasks.FindAsync(taskId);
            if (task == null) return;
            task.TimeWindowsJson = windows.Any()
                ? System.Text.Json.JsonSerializer.Serialize(windows)
                : null;
            task.LastModified = DateTime.UtcNow;
            await ctx.SaveChangesAsync();
            _cache.Remove(typeof(TaskEntity).Name);
            _cache.Remove($"{typeof(TaskEntity).Name}_{taskId}");
        }

        private static Task<bool> CheckTeamAutoacceptAsync(Team team) => Task.FromResult(false);

        private static void PushTaskScheduled(ApplicationDbContext ctx, TaskEntity task, Team team)
        {
            var hierarchy = new TaskHierarchy
            {
                TaskId = task.Id,
                TeamId = team.Id,
                Timestamp = DateTime.UtcNow
            };
            ctx.TaskHierarchy.Add(hierarchy);
        }

        private static bool HasHierarchyHops(ApplicationDbContext ctx, TaskEntity task) =>
            ctx.TaskHierarchy.Any(h => h.TaskId == task.Id);

        private static Team PopTaskScheduled(ApplicationDbContext ctx, TaskEntity task)
        {
            var lastHop = ctx.TaskHierarchy
                .Where(h => h.TaskId == task.Id)
                .Include(h => h.Team)
                .OrderBy(h => h.Timestamp)
                .ToList()
                .Last();
            ctx.TaskHierarchy.Remove(lastHop);
            return lastHop.Team;
        }

        private static void LogTaskStatusChange(ApplicationDbContext ctx, TaskEntity task, Team? team, UserAtak user, TaskStatusTak previous, TaskStatusTak next, string description)
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
            ctx.TaskLogs.Add(log);
        }
    }
}
