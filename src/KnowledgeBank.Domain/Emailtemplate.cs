using System.ComponentModel.DataAnnotations;

namespace KnowledgeBank.Domain
{
    public class Emailtemplate : Entity
    {
        [Required]
        public string Content { get; set; }
        public long CaseId { get; set; }
        public Case Case { get; set; }
    }
}