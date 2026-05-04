
namespace TAKSuite.Data.Models
{
    public class MissionSuite: IGuidModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string? Descrizione { get; set; }
        
        public MissionStatusTak Status { get; set; } = MissionStatusTak.Created;


        public string? TakMissionUid { get; set; }
        // ATAK Link
        public List<string> MissionUids { get; set; } = new(); // Lista degli UID (ATAK) di missione





        // Sotto task e gerarchia (per ora non sviluppati)
        public List<TaskEntity> Tasks { get; set; } = new();


        // Relazione gerarchica: lista dei team associati alla missione
        public List<Team> Teams { get; set; } = new();



        // Controllo date
        public DateTime CreationDate { get; set; } = DateTime.UtcNow;
        public DateTime LastModified { get; set; } = DateTime.UtcNow;
    }
}
