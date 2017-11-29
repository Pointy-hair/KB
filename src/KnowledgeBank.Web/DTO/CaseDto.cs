using KnowledgeBank.Domain;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;

namespace KnowledgeBank.Web.DTO
{
	public class CaseDto : IEntity
    {
		public long Id { get; set; }
		public string Title { get; set; }
		public string Category { get; set; }
		public long AreaId { get; set; }
		public DateTimeOffset CreatedAt { get; set; }
		public DateTimeOffset LastModifiedAt { get; set; }
		public IEnumerable<TagDto> Tags { get; set; }
		public IEnumerable<StepDto> Steps { get; set; }
		public IEnumerable<LinkDto> Links { get; set; }
		public IEnumerable<AttachmentDto> Attachments { get; set; }
        public IEnumerable<EmailTemplateDto> EmailTemplates { get; set; }
    }

    public static partial class Dto
    {

        public static CaseDto FromModel(Case model)
        {
            return new CaseDto
            {
                Id = model.Id,
                Title = model.Title,
                Category = model.Category,
                AreaId = model.TenantId,
                CreatedAt = model.CreatedAt,
                LastModifiedAt = model.LastModifiedAt,
                Tags = model.Tags?.Select(t => FromModel(t)) ?? new TagDto[0],
                Steps = model.Steps?.Select(s => FromModel(s)) ?? new StepDto[0],
                Links = model.Links?.Select(link => FromModel(link)) ?? new LinkDto[0],
                Attachments = model.Attachments?.Select(a => FromModel(a)) ?? new AttachmentDto[0],
				EmailTemplates = model.EmailTemplates?.Select(FromModel)
			};
        }


        public static Case ToModel(this CaseDto dto)
        {
            return new Case
            {
                Id = dto.Id,
                Title = dto.Title,
                Category = dto.Category,
                TenantId = dto.AreaId,
                CreatedAt = dto.CreatedAt,
                LastModifiedAt = dto.LastModifiedAt,
                Tags = dto.Tags.Select(t => t.ToModel()).ToList(),
                Steps = dto.Steps.Select(s => s.ToModel()).ToList(),
                Links = dto.Links.Select(link => link.ToModel()).ToList(),
                Attachments = dto.Attachments.Select(a => a.ToModel()).ToList(),
				EmailTemplates = dto.EmailTemplates.Select(ToModel).ToList()
			};
        }
    }
}
