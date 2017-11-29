using KnowledgeBank.Domain;

namespace KnowledgeBank.Web.DTO
{
    public class EmailTemplateDto : IEntity
    {
        public long Id { get; set; }
        public string Content { get; set; }
    }

    public static partial class Dto
    {
        public static EmailTemplateDto FromModel(Emailtemplate model)
        {
            return new EmailTemplateDto
            {
                Id = model.Id,
                Content = model.Content
            };
        }

        public static Emailtemplate ToModel(this EmailTemplateDto dto)
        {
            return new Emailtemplate
            {
                Id = dto.Id,
                Content = dto.Content
            };
        }
    }
}
