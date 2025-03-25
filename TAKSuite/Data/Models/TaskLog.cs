namespace TAKSuite.Data.Models
{
    public class TaskLog
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid TaskId { get; set; }
        public TaskEntity Task { get; set; } = null!;
        public Guid? TeamId { get; set; } // Team che ha eseguito l'azione
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public TaskStatusTak PreviousStatus { get; set; }
        public TaskStatusTak NewStatus { get; set; }
        public string ActionDescription { get; set; } = string.Empty;
    }
}
