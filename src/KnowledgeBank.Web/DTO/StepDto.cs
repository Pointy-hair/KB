using KnowledgeBank.Domain;

namespace KnowledgeBank.Web.DTO
{
    public class StepDto : IEntity
    {
        public long Id { get; set; }
        public string Description { get; set; }
    }

    public static partial class Dto
    {
        public static StepDto FromModel(Step model)
        {
            return new StepDto
            {
                Id = model.Id,
                Description = model.Description
            };
        }

        public static Step ToModel(this StepDto dto)
        {
            return new Step
            {
                Id = dto.Id,
                Description = dto.Description
            };
        }
    }
}