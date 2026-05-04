namespace TAKSuite.Data.Models
{
    public interface IStringValue
    {
        public string Value { get; set; }
        public Guid TaskEntityId { get; set; }
        public TaskEntity Task { get; set; }
    }
}
