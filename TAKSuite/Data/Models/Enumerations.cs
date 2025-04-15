using System.ComponentModel.DataAnnotations;

namespace TAKSuite.Data.Models
{
    public enum TaskStatusTak
    {
        None = 0,
        
        [Display(Name = "Creato")]
        Created = 1,        // task is created from DE
        
        [Display(Name = "Schedulato")] 
        Scheduled = 2,      // task is scheduled to a Team
        
        [Display(Name = "Accettato")] 
        Accepted = 3,       // task is accepted by the Team and ready to be assigned
        Assigned = 4,       // task is assigned to the Executor team
        
        [Display(Name = "In Esecuzione")] 
        InProgress = 5,     // task is Executed by a team
        
        [Display(Name = "Completato")] 
        Completed = 6,      // task has been completed
        RejectedTier1 = 7,  // task has been rejected from Scheduled / Accepted
        RejectedTier2 = 8,  // task has been rejected from Assigned
        Aborted = 9,        // task has been Aborted
        Failed = 10,        // task has been Failed      
        Canceled = 11       // task has been Canceled normally by DE or Tier1
    }

}
