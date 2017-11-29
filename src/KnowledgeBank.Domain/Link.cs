using System.ComponentModel.DataAnnotations;

namespace KnowledgeBank.Domain
{
    public class Link : Entity
    {
        public long CaseId { get; set; }
        public Case Case { get; set; }
        [Required]
        public string Location { get; set; }
        public string Description { get; set; }
    }
}
