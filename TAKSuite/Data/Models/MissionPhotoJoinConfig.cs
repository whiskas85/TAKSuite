namespace TAKSuite.Data.Models
{
    public class MissionPhotoJoinConfig
    {
        public Guid Id { get; set; }
        public Guid MissionId { get; set; }
        public MissionSuite Mission { get; set; } = null!;

        public bool Enabled { get; set; }
        public int RadiusMeters { get; set; } = 50;

        public List<CotPriorityRule> PriorityRules { get; set; } = new();
    }
}
