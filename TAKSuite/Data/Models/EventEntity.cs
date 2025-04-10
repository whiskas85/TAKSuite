namespace TAKSuite.Data.Models
{
    public class EventEntity: IGuidModel
    {
        public Guid Id { get; set; }
        public DateTime? Timestamp { get; set; } = DateTime.Now;
        public string? Title { get; set; }
        public string? Note { get; set; }
        public DateTime CreatedTime { get; set; } = DateTime.Now;
        public DateTime LastEditTime { get; set; }
    }
}
