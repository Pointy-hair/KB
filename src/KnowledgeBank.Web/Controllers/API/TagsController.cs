using KnowledgeBank.Domain;
using KnowledgeBank.Persistence.Repositories;
using KnowledgeBank.Web.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using KnowledgeBank.Web.Helpers;

namespace KnowledgeBank.Web.Controllers.API
{
	[Authorize]
	[Route("[controller]")]
	[Branch(Branch.Api)]
	public class TagsController : ControllerBase
	{
		public IRepository<Tag> _repository { get; set; }

		public TagsController(IRepository<Tag> repository)
		{
			this._repository = repository;
		}

		[HttpGet]
		public IEnumerable<TagDto> Get([FromQuery]long tenant)
		{
			return _repository.Read().Where(tag => tag.Case.TenantId == tenant).Select(m => Dto.FromModel(m)).ToList();
		}
	}
}
