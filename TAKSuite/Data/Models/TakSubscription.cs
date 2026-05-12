namespace TAKSuite.Data.Models
{
    public class TakSubscription
    {
        public Guid   Id           { get; set; } = Guid.NewGuid();
        public string MissionName  { get; set; } = "";
        public DateTime SubscribedAt { get; set; } = DateTime.UtcNow;
    }
}
