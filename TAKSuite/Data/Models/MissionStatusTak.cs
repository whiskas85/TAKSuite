using System.ComponentModel.DataAnnotations;

namespace TAKSuite.Data.Models
{
    public enum MissionStatusTak
    {
        None = 0,

        [Display(Name = "Creata")]
        Created = 1,        // mission has been created

        [Display(Name = "Assegnata")]
        Assigned = 4,       // mission has been assigned to a team

        [Display(Name = "In Esecuzione")]
        InProgress = 10,     // mission is in execution
        [Display(Name = "In Pausa")]
        Paused = 11,     // mission is in paused
        [Display(Name = "Stoppata")]
        Stopped = 12,     // mission is in stopped


        [Display(Name = "Completata")]
        Completed = 50,      // Completed
        
        [Display(Name = "Cancellata")]
        Canceled = 51,      // Canceled normally by DE or Tier1
        
        [Display(Name = "Archiviata")]
        Archived = 52,     // Archiviata normally by DE or Tier1
    }
}