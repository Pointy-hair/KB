namespace KnowledgeBank.Domain
{
    public class Step : Entity
    {
        public int OrderNumber { get; set; }
        public string Description { get; set; }
        public long CaseId { get; set; }
        public Case Case { get; set; }
    }
}
