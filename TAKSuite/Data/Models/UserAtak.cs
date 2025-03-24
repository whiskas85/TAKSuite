
using System.ComponentModel.DataAnnotations;

namespace TAKSuite.Data.Models
{
    public class UserAtak : IGuidModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = "";
        public string Surname { get; set; } = "";
        public string Email { get; set; } = "";
        public string PhoneNumber { get; set; } = "";
        public string Nick { get; set; } = "";

        // Relazione uno-a-molti (ogni utente appartiene a un team)
        public Guid? TeamId { get; set; }
        public Team? Team { get; set; }

        // Relazione uno-a-uno (se questo utente è leader di un team)
        public Team? LedTeam { get; set; }
    }
}
