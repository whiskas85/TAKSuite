namespace TAKSuite.Data.Models
{
    public class Documentation: IGuidModel
    {
        public Guid Id { get; set; }
        public String Name { get; set; } = "";
        public String Type { get; set; } = "";
        public String Path { get; set; } = "";

        public DateTime LastModified { get; set; } = DateTime.Now;
        public DateTime CreationDate { get; set; } = DateTime.Now;
        public bool IsValid { get; set; } = true;

        // Relazione molti-a-molti con Owner (Team, Person, etc.)
        public List<DocumentationOwner> DocumentationOwners { get; set; } = new();

    }
}
