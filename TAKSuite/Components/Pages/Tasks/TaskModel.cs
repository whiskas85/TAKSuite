using System.ComponentModel.DataAnnotations;

namespace TAKSuite.Components.Pages.Tasks
{


    public class TaskModel
    {
        public Guid? Id { get; set; }

        [Required(ErrorMessage = "Il nome del task è obbligatorio.")]
        [StringLength(100, ErrorMessage = "Il nome non può superare i 100 caratteri.")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "La descrizione è obbligatoria.")]
        [StringLength(500, ErrorMessage = "La descrizione non può superare i 500 caratteri.")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Devi selezionare una squadra.")]
        public Guid? AssignedTeamId { get; set; }

    }
}


