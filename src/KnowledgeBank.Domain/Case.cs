using System;
using System.Collections.Generic;

namespace KnowledgeBank.Domain
{
    public interface ICase
    {
        ICollection<Step> Steps { get; set; }
        ICollection<Link> Links { get; set; }
        ICollection<Attachment> Attachments { get; set; }
        ICollection<Emailtemplate> EmailTemplates { get; set; }
        ICollection<Tag> Tags { get; set; }
    }
    public class Case : Entity
    {
        public string Title { get; set; }
        public string Category { get; set; }
        public long TenantId { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset LastModifiedAt { get; set; }
        public ICollection<Tag> Tags { get; set; } = new List<Tag>();
        public ICollection<Step> Steps { get; set; } = new List<Step>();
        public ICollection<Link> Links { get; set; } = new List<Link>();
        public ICollection<Attachment> Attachments { get; set; } = new List<Attachment>();
        public ICollection<Emailtemplate> EmailTemplates { get; set; } = new List<Emailtemplate>();
        public bool IsDeleted { get; set; }
    }
}
