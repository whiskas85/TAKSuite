namespace TAKSuite.Data.Models
{
    /// <summary>
    /// Template COT riutilizzabile per generare punti TAK tipizzati.
    /// Il campo CotXml contiene il CoT Event completo con i seguenti placeholder:
    ///   {{UID}}         — UUID generato
    ///   {{NAME}}        — callsign / nome del punto
    ///   {{LAT}}         — latitudine WGS84 decimale
    ///   {{LON}}         — longitudine WGS84 decimale
    ///   {{REMARKS}}     — note opzionali
    ///   {{MISSION_UID}} — UID missione TAK
    ///   {{TIME}}        — timestamp ISO corrente
    ///   {{STALE}}       — timestamp di scadenza (TIME + 1 anno)
    ///   {{CREATOR_UID}} — UID creatore TAK
    /// </summary>
    public class CotTemplate
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string CotXml { get; set; } = string.Empty;
        public string? IconPreview { get; set; }  // emoji o testo breve per UI (es. "🎯", "WP", "🚁")
        public bool IsPrimary { get; set; }       // true = l'AI usa questo template come punto "principale" (OBJ)
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
