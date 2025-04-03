namespace TAKSuite.Data.Models
{
    public class Team : IGuidModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public int Color { get; set; }
        public int Points { get; set; }

        // Relazione uno-a-uno per il Team Leader
        public Guid? TeamLeaderId { get; set; }
        public UserAtak? TeamLeader { get; set; }

        // Relazione uno-a-molti per gli utenti appartenenti al team
        public List<UserAtak> Members { get; set; } = new();

        // Relazione uno-a-molti per i canali radio
        public List<TeamRadioChannel> RadioChannelsList { get; set; } = new();

        // Relazione gerarchica: riferimento al team padre
        public Guid? ParentTeamId { get; set; }
        public Team? ParentTeam { get; set; }

        // Relazione gerarchica: lista dei sotto-team
        
        public List<Team> SubTeams { get; set; } = new();
        public string MissionUid { get; set; }
    }
}
