using KnowledgeBank.Domain;
using System.ComponentModel.DataAnnotations;

namespace KnowledgeBank.Web.DTO
{
    public class LinkDto : IEntity
    {
        public long Id { get; set; }
        [Required]
        public string Location { get; set; }
        public string Description { get; set; }
    }

    public partial class Dto
    {
        public static LinkDto FromModel(Link model)
        {
            return new LinkDto
            {
                Id = model.Id,
                Location = model.Location,
                Description = model.Description
            };
        }

        public static Link ToModel(this LinkDto dto)
        {
            return new Link
            {
                Id = dto.Id,
                Location = dto.Location,
                Description = dto.Description
            };
        }
    }
}