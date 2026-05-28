using System.ComponentModel.DataAnnotations;

namespace TAKSuite.Data.Models;

public class CachedCoTEntry
{
    [Key]
    public string  Uid      { get; set; } = "";
    public string? Callsign { get; set; }
    public string? CotType  { get; set; }
    public double  Lat      { get; set; }
    public double  Lon      { get; set; }
    public double  Hae      { get; set; }
    public string? Team     { get; set; }
    public string? Role     { get; set; }
    public string  RawXml   { get; set; } = "";
    public string?   MissionName { get; set; }
    public DateTime FirstSeen   { get; set; }
    public DateTime LastSeen    { get; set; }
}
