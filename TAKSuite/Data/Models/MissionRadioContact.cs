namespace TAKSuite.Data.Models
{
    public class MissionRadioContact : IGuidModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = "";
        public Guid MissionId { get; set; }
        public MissionSuite? Mission { get; set; }
        public Guid RadioChannelId { get; set; }
        public RadioChannel RadioChannel { get; set; } = null!;
        public Guid? BackupRadioChannelId { get; set; }
        public RadioChannel? BackupRadioChannel { get; set; }
        public string? Note { get; set; }
    }
}
