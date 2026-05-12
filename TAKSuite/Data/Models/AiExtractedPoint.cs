namespace TAKSuite.Data.Models
{
    public enum AiPointType { Waypoint, Objective }

    public class AiExtractedPoint
    {
        public string Name { get; set; } = string.Empty;
        public AiPointType Type { get; set; } = AiPointType.Waypoint;
        public double Lat { get; set; }
        public double Lon { get; set; }
        public string? Notes { get; set; }
        public bool Selected { get; set; } = true;
        public int ArgbColor { get; set; } = -65536; // Red default
    }
}
