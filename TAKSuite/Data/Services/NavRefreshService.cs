namespace TAKSuite.Data.Services;

public class NavRefreshService
{
    public event Action? OnNavChanged;
    public void NotifyNavChanged() => OnNavChanged?.Invoke();

    public event Action<Guid, string>? OnTaskNameChanged;
    public void NotifyTaskNameChanged(Guid taskId, string newName) => OnTaskNameChanged?.Invoke(taskId, newName);
}
