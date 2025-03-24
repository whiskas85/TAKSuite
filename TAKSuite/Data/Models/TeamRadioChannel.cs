namespace TAKSuite.Data.Models
{
    public class TeamRadioChannel : IGuidModel
    {
        public Guid Id { get; set; }
        public ChannelType Position { get; set; }
        public string Name { get; set; }
        public Guid TeamId { get; set; }
        public Team Team { get; set; }
        public Guid RadioChannelId { get; set; }
        public RadioChannel RadioChannel { get; set; }

        public Guid? BackupRadioChannelId { get; set; }
        public RadioChannel? BackupRadioChannel { get; set; }


        public DateTime? BeginValidityPeriod { get; set; }
        public DateTime? EndValidityPeriod { get; set; }
    }
    public enum ChannelType
    {
        BaseChannel = 0,
        TeamLeaderChannel = 1,
        ViceTeamLeaderChannel = 2,
    }
}
