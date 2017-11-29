using System.ComponentModel.DataAnnotations;

namespace KnowledgeBank.Domain
{
    public class Attachment : Entity
    {
        [Required]
        public string Location { get; set; }
        [Required]
        public string Name { get; set; }
        public long CaseId { get; set; }
        public Case Case { get; set; }
        public int Size { get; set; }
    }
}
