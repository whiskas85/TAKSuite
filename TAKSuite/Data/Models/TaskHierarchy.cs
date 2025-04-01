namespace TAKSuite.Data.Models
{
    public class TaskHierarchy
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid TaskId { get; set; }
        public TaskEntity Task { get; set; } = null!;
        
        public Guid? TeamId { get; set; } // Team schedulato
        public Team? Team { get; set; } // Team schedulato

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
