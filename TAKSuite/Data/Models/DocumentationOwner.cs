namespace TAKSuite.Data.Models
{
    public class DocumentationOwner
    {
        public Guid DocumentationId { get; set; }
        public Documentation Documentation { get; set; } = null!;

        public Guid OwnerId { get; set; }  // Chiave esterna per la classe "Owner" (Team, Person, ecc.)
        public string OwnerType { get; set; }  // Tipo di entità (Team, Person, ecc.)

        // Ulteriori proprietà che potrebbero essere necessarie per la relazione
        public DateTime AddedDate { get; set; }  // Data in cui il documento è stato associato all'entità
    }
}
