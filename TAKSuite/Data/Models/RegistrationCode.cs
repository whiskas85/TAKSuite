namespace TAKSuite.Data.Models
{
    public class RegistrationCode : IGuidModel
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public Guid TeamId { get; set; }
        public Team Team { get; set; }


        public bool IsValid { get; set; } = true;
        public DateTime? ExpirationDate { get; set; }
    }
}
