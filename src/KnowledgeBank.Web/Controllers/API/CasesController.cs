using KnowledgeBank.Domain;
using KnowledgeBank.Persistence.Repositories;
using KnowledgeBank.Web.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using KnowledgeBank.Web.Helpers;
using Microsoft.AspNetCore.Http;
using System.IO;
using KnowledgeBank.Web.Settings;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;

namespace KnowledgeBank.Web.Controllers.API
{
	[Authorize]
	[Route("[controller]")]
	[Branch(Branch.Api)]
	public class CasesController : ControllerBase
	{
		public CaseRepository _repository { get; set; }


        private readonly AssetSettings _assetSettings;

		public CasesController(CaseRepository repository, IOptions<AssetSettings> settings)
		{
			this._repository = repository;
            this._assetSettings = settings.Value;
		}

        [HttpGet("{id}")]
        public IActionResult Get(long id)
        {
            var @case = _repository.Read()
                .Include(x => x.Tags)
                .Include(x => x.Steps)
                .Include(x => x.Links)
                .Include(x => x.Attachments)
				.Include(x => x.EmailTemplates)
                .Where(x => x.Id == id && x.IsDeleted == false)
                .Select(x => Dto.FromModel(x))
                .FirstOrDefault();

            if (@case is null)
                return NotFound();
            return Ok(@case);
        }

		[HttpGet]
		public List<CategoryDto> Get(string query)
		{
			var queryable = _repository.Read().Where(c => c.IsDeleted == false);

			if (!string.IsNullOrWhiteSpace(query))
			{
                queryable = queryable.Where(c =>
                    c.Category.Contains(query) ||
                    c.Title.Contains(query) ||
                    c.Tags.Any(tag => tag.Name.Contains(query)) ||
                    c.Steps.Any(step => step.Description.Contains(query)));
			}

			var casesByCategory = queryable
				.GroupBy(c => c.Category)
				.Select(g => new CategoryDto { Name = g.Key, Cases = g.Select(x => Dto.FromModel(x)) })
				.ToList();
			return casesByCategory;
		}

        [HttpPost]
        [Authorize(Roles = Role.AreaAdmin + ", " + Role.ReadWriteUser)]
        public async Task<ActionResult> Post(List<IFormFile> attachments, [FromForm]string dto)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            if (dto == null)
            {
                return BadRequest($"No '{nameof(dto)}' form data was provided");
            }

            CaseDto caseDto;
            try
            {
                caseDto = Newtonsoft.Json.JsonConvert.DeserializeObject<CaseDto>(dto);
            }
            catch (Exception)
            {
                return BadRequest();
            }

            long tenantId = _repository.GetTenantId();
            var tasks = attachments.Select(f => SaveAttachment(f, tenantId, caseDto.Attachments.First(a => a.Name == f.FileName)));
            await Task.WhenAll(tasks);

            caseDto.CreatedAt = DateTimeOffset.UtcNow;
            caseDto.LastModifiedAt = DateTimeOffset.UtcNow;
            Case newCase = caseDto.ToModel();

            caseDto.Id = _repository.Create(newCase);
            return Created(newCase.Id.ToString(), caseDto);
        }

        [Authorize(Roles = Role.AreaAdmin + ", " + Role.ReadWriteUser)]
        [HttpPut]
        public async Task<ActionResult> Put(List<IFormFile> attachments, [FromForm]string dto)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            if (dto == null)
            {
                return BadRequest($"No '{nameof(dto)}' form data was provided");
            }

            CaseDto caseDto;
            try
            {
                caseDto = Newtonsoft.Json.JsonConvert.DeserializeObject<CaseDto>(dto);
            }
            catch (Exception)
            {
                return BadRequest();
            }

            var dbCase = _repository.Read()
                .Include(x => x.Attachments)
                .Include(x => x.Tags)
                .Include(x => x.Steps)
                .Include(x => x.EmailTemplates)
                .Include(x => x.Links)
                .FirstOrDefault(x => x.Id == caseDto.Id);
            long tenantId = _repository.GetTenantId();
            var tasks = attachments.Select(f => SaveAttachment(f, tenantId, caseDto.Attachments.First(a => a.Name == f.FileName)));
            await Task.WhenAll(tasks);

            caseDto.LastModifiedAt = DateTimeOffset.UtcNow;
                _repository.UpdateCollection(dbCase, x => x.Attachments, caseDto.Attachments, x => x.ToModel(), false);
                _repository.UpdateCollection(dbCase, x => x.Tags, caseDto.Tags, x => x.ToModel());
                _repository.UpdateCollection(dbCase, x => x.Links, caseDto.Links, x => x.ToModel());
                _repository.UpdateCollection(dbCase, x => x.Steps, caseDto.Steps, x => x.ToModel());
                _repository.UpdateCollection(dbCase, x => x.EmailTemplates, caseDto.EmailTemplates, x => x.ToModel());
                _repository.Update(dbCase, caseDto);

            return Ok();
        }

        [HttpDelete("{id}")]
        public ActionResult Delete(long id)
        {
            var deletedCase = _repository.Get(id);

            if (deletedCase is null)
                return NotFound();

            deletedCase.IsDeleted = true;
            _repository.Update(deletedCase);

            return NoContent();
        }

		[HttpGet("categories")]
		public IEnumerable<string> GetCategories()
		{
			return _repository.Read().Select(c => c.Category).Distinct();
		}

        [Route("statsForArea")]
        public AreaStats StatsForArea(long tenant)
        {
            var result =  new AreaStats
            {
                AreaId = tenant,
                CaseCount = _repository.Read().Count(),
                CategoryCount = _repository.Read().Select(c => c.Category).Distinct().Count()
            };
            return result;
        }

        /// <summary>
        /// Saves the attachment to disk and updates the location <paramref name="dto"/>
        /// </summary>
        private async Task SaveAttachment(IFormFile file, long tenantId, AttachmentDto dto)
        {
            string extension = Path.GetExtension(file.FileName);
            string actualStorePath = $"{_assetSettings.AssetStorePath}/{tenantId}";

            string assetName;
            string assetStorePath;
            do
            {
                assetName = $"{Guid.NewGuid()}{extension}";
                assetStorePath = $"{actualStorePath}/{assetName}";
            }
            while (System.IO.File.Exists(assetStorePath)); // there's a chance that the same Guid was generated before

            if (!Directory.Exists(actualStorePath))
                Directory.CreateDirectory(actualStorePath);

            if (file.Length > 0)
                using (var stream = new FileStream(assetStorePath, FileMode.Create))
                    await file.CopyToAsync(stream);

            dto.Location = assetStorePath;
        }
    }

	public class CategoryDto
	{
		public string Name { get; set; }
		public IEnumerable<CaseDto> Cases { get; set; }
	}

    public class AreaStats
    {
        public long AreaId { get; set; }
        public int CaseCount { get; set; }
        public int CategoryCount { get; set; }
    }

    public class CaseWithAttachments
    {
        public CaseDto Dto { get; set; }
        public List<IFormFile> Attachments { get; set; }
    }
}
