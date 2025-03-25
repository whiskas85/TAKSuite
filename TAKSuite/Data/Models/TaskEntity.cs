namespace TAKSuite.Data.Models
{
    public class TaskEntity: IGuidModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public List<string> Uids { get; set; } = new(); // Lista di UID (ATAK)
        public List<Documentation> Documents { get; set; } = new(); // Documenti allegati

        public Guid? AssignedTeamId { get; set; } // Squadra assegnata
        public Team? AssignedTeam { get; set; }

        public Guid? ExecutingTeamId { get; set; } // Squadra che esegue il task
        public Team? ExecutingTeam { get; set; }

        public DateTime CreationDate { get; set; } = DateTime.UtcNow;
        public DateTime LastModified { get; set; } = DateTime.UtcNow;
        public DateTime? AssignedDate { get; set; }
        public DateTime? CompletedDate { get; set; }

        public TaskStatusTak Status { get; set; } = TaskStatusTak.Created;
        public List<TaskLog> Logs { get; set; } = new();
    }
}
