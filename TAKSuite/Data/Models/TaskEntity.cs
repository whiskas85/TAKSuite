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
        public string? PoiUid { get; set; }             // UID del POI principale (legacy — usare PoiLinks)
        public string? PoiName { get; set; }            // Nome del POI principale (legacy — usare PoiLinks)

        // Multi-point support: punto principale + punti secondari (ingresso, uscita, eliporto…)
        public string? PoiLinksJson { get; set; }

        [NotMapped]
        public List<TaskPoiLink> PoiLinks
        {
            get
            {
                if (!string.IsNullOrEmpty(PoiLinksJson))
                {
                    try { return System.Text.Json.JsonSerializer.Deserialize<List<TaskPoiLink>>(PoiLinksJson) ?? new(); }
                    catch { return new(); }
                }
                // Backward compat: se non c'è JSON ma c'è il vecchio PoiUid, crea il link principale
                if (!string.IsNullOrEmpty(PoiUid))
                    return new List<TaskPoiLink> { new() { Uid = PoiUid, Name = PoiName ?? PoiUid, IsPrimary = true, Role = "Principale" } };
                return new();
            }
            set
            {
                PoiLinksJson = System.Text.Json.JsonSerializer.Serialize(value);
                // Mantieni PoiUid/PoiName sincronizzati con il link principale
                var primary = value.FirstOrDefault(p => p.IsPrimary) ?? value.FirstOrDefault();
                PoiUid  = primary?.Uid;
                PoiName = primary?.Name;
            }
        }


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

        // Finestra temporale obiettivo
        public DateTime? StartDateTime { get; set; }
        public DateTime? EndDateTime { get; set; }
        public DateTime? GreenLightDateTime { get; set; }

        // Forchetta pianificata (neutrale, non operativa)
        public DateTime? PlannedStartDateTime { get; set; }
        public DateTime? PlannedEndDateTime { get; set; }

        // Finestre temporali multiple (fino a 10), serializzate come JSON
        public string? TimeWindowsJson { get; set; }

        [NotMapped]
        public List<TaskTimeWindow> TimeWindows
        {
            get
            {
                if (string.IsNullOrEmpty(TimeWindowsJson)) return new();
                try { return System.Text.Json.JsonSerializer.Deserialize<List<TaskTimeWindow>>(TimeWindowsJson) ?? new(); }
                catch { return new(); }
            }
            set => TimeWindowsJson = System.Text.Json.JsonSerializer.Serialize(value);
        }




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
