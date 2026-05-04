using System.ComponentModel.DataAnnotations.Schema;

namespace TAKSuite.Data.Models
{
    public class TaskEntity: IGuidModel
    {
        // Dettaglio di missione
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int Points { get; set; } = 0;
        public Guid? PriorityId { get; set; }
        public TaskPriority? Priority { get; set; }



        // ATAK Link
        public List<string> Uids { get; set; } = new(); // Lista di UID (ATAK) allegati al task
        public string MissionUid { get; set; } = "";    // Missione dalla quale attingere


        public Guid? MissionTAKSuiteId { get; set; }       // FK esplicita
        public MissionSuite? MissionTAKSuite { get; set; }


        // Documentazione
        public List<Documentation> Documents { get; set; } = new(); // Documenti allegati


        // Assegnazioni
        public Guid? AssignedTeamId { get; set; }   // Squadra assegnata come proprietaria del TASK
        public Team? AssignedTeam { get; set; }

        public Guid? ExecutingTeamId { get; set; }  // Squadra che esegue materialmente il task
        public Team? ExecutingTeam { get; set; }






        public Guid? RadioChannelId { get; set; }     // ✅ FK
        public RadioChannel? RadioChannel { get; set; } // ✅ Navigation

        public string Sottotitolo { get; set; } = string.Empty;

        public string TipologiaObiettivo { get; set; } = string.Empty;
        public int? Durata { get; set; }




        // ✅ UNICA COLLECTION MAPPATA
        public List<TaskStringItem> Items { get; set; } = new();



        // Controllo date
        public DateTime CreationDate { get; set; } = DateTime.UtcNow;
        public DateTime LastModified { get; set; } = DateTime.UtcNow;

        public TaskStatusTak Status { get; set; } = TaskStatusTak.Created;
        
        
        // Log contenenti le date dei cambi di stato
        public List<TaskLog> Logs { get; set; } = new();


        // Sotto task e gerarchia (per ora non sviluppati)
        public List<TaskHierarchy> Hierarchy { get; set; } = new();

    }
}
