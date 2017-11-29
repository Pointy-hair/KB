namespace KnowledgeBank.Domain
{
    public class Tag : Entity
    {
        public string Name { get; set; }
        public long CaseId { get; set; }
        public Case Case { get; set; }
    }
}
