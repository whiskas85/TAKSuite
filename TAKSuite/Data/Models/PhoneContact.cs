namespace TAKSuite.Data.Models
{
    public class PhoneContact : IGuidModel
    {
        public Guid Id { get; set; }
        public string Nome { get; set; } = "";
        public string Cognome { get; set; } = "";
        public string? Nickname { get; set; }
        public string? Telefono { get; set; }
        public string? Squadra { get; set; }
    }
}
