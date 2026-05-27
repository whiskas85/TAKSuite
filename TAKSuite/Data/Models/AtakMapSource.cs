namespace TAKSuite.Data.Models;

public class AtakMapSource
{
    public int Id { get; set; }
    [System.ComponentModel.DataAnnotations.MaxLength(450)]
    public string FileName { get; set; } = "";
    public string Name { get; set; } = "";
    public string SourceType { get; set; } = "XYZ";
    public string Url { get; set; } = "";
    public string? Layers { get; set; }
    public int MinZoom { get; set; }
    public int MaxZoom { get; set; } = 19;
    public string? Attribution { get; set; }
    public bool IsHidden { get; set; }
    public DateTime ImportedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
