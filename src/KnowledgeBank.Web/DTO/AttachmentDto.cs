using KnowledgeBank.Domain;
using System.ComponentModel.DataAnnotations;

namespace KnowledgeBank.Web.DTO
{
    public class AttachmentDto : IEntity
    {
        public long Id { get; set; }
        [Required]
        public string Location { get; set; }
        [Required]
        public string Name { get; set; }
        public int Size { get; set; }
    }

    public partial class Dto
    {
        public static AttachmentDto FromModel(Attachment model)
        {
            return new AttachmentDto
            {
                Id = model.Id,
                // Location is for internal use
                Name = model.Name,
                Size = model.Size
            };
        }

        public static Attachment ToModel(this AttachmentDto dto)
        {
            return new Attachment
            {
                Id = dto.Id,
                Location = dto.Location,
                Name = dto.Name,
                Size = dto.Size
            };
        }
    }
}
