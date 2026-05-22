namespace TAKSuite.Data.Models
{
    public enum DocumentTypeTarget
    {
        Mission = 0,
        Task = 1,
        Team = 2
    }

    public class DocumentType : IGuidModel
    {
        public Guid Id { get; set; }
        public string Description { get; set; } = "";
        public DocumentTypeTarget Target { get; set; } = DocumentTypeTarget.Mission;
        public string? Icon { get; set; }  // IconName enum name, e.g. "FolderFill"
        public int Order { get; set; }
    }
}
