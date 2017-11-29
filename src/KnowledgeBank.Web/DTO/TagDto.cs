using KnowledgeBank.Domain;

namespace KnowledgeBank.Web.DTO
{
    public class TagDto : IEntity
    {
        public long Id { get; set; }
        public string Name { get; set; }
    }

    public static partial class Dto
    {
        public static TagDto FromModel(Tag model)
        {
            return new TagDto
            {
                Id = model.Id,
                Name = model.Name,
            };
        }

        public static Tag ToModel(this TagDto dto)
        {
            return new Tag
            {
                Id = dto.Id,
                Name = dto.Name
            };
        }
    }
}