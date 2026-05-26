namespace TAKSuite.Data.Services;

public class NavRefreshService
{
    public event Action? OnNavChanged;
    public void NotifyNavChanged() => OnNavChanged?.Invoke();
}
