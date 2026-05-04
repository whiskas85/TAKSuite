using TAKSuite.Data.Models;

public class TaskStringItem
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Value { get; set; } = string.Empty;
    
    public TaskStringItemType Type { get; set; }
    public Guid TaskEntityId { get; set; }
    public TaskEntity Task { get; set; }
}