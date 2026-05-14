namespace TAKSuite.Data.Models
{
    public class CotPriorityRule
    {
        public Guid Id { get; set; }
        public Guid PhotoJoinConfigId { get; set; }
        public MissionPhotoJoinConfig PhotoJoinConfig { get; set; } = null!;

        public int Priority { get; set; }
        public string? TypeContains { get; set; }
        public string? CallSignContains { get; set; }

        // Tipi esclusi separati da virgola, es: "a-f-G,friendly"
        public string? ExcludedTypesCsv { get; set; }

        public List<string> ExcludedTypes =>
            string.IsNullOrWhiteSpace(ExcludedTypesCsv)
                ? new()
                : ExcludedTypesCsv.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();
    }
}
