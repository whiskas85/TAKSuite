namespace TAKSuite.Data.Models
{
    public class TaskPoiLink
    {
        public string Uid { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public bool IsPrimary { get; set; }
        public string Role { get; set; } = "Principale";  // es. Principale, Ingresso, Uscita, Eliporto
        public Guid? TemplateId { get; set; }              // template COT usato per generare il punto
    }
}
