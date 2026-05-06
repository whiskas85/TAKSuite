using BlazorBootstrap;

namespace TAKSuite.Data.Models
{
    public class TaskPriority: IGuidModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = "";
        public int Level { get; set; }
        public CardColor CardColor { get; set; }
        public bool IsDefault { get; set; }
    }
}