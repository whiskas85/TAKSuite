using TAKSuite.Data.Models;

namespace TAKSuite.Components.Pages.ManagingTeamsHierachy
{
    public class TreeNodeModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = "";

        public NodeType NodeType { get; set; }


        public Guid? ParentId { get; set; }
        public TreeNodeModel? Parent { get; set; }
        public List<TreeNodeModel> Children { get; set; } = new();

        public Team Item { get; set; }
        public bool AllowChildren { get; set; } = false;
    }
    public enum NodeType
    {
        Team,
        Operator,
        Documentation,
        Task,
    }
}
