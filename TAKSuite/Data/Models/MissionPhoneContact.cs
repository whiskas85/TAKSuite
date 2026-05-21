namespace TAKSuite.Data.Models
{
    public class MissionPhoneContact : IGuidModel
    {
        public Guid Id { get; set; }
        public Guid MissionId { get; set; }
        public MissionSuite? Mission { get; set; }
        public Guid PhoneContactId { get; set; }
        public PhoneContact PhoneContact { get; set; } = null!;
        public string Role { get; set; } = "";
        public string? Description { get; set; }
    }
}
