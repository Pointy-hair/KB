using IdentityModel;
using KnowledgeBank.Persistence;
using KnowledgeBank.Web.DTO;
using KnowledgeBank.Web.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using KnowledgeBank.Web.Helpers;

namespace KnowledgeBank.Web.Controllers
{
	[Authorize]
	[Route("[controller]")]
	[Branch(Branch.Info)]
	public class UserInfoController : ControllerBase
	{
		private readonly ApplicationIdentityDbContext _dbContext;

		public UserInfoController(ApplicationIdentityDbContext dbContext)
		{
			_dbContext = dbContext;
		}


		[HttpGet("areas")]
		public IEnumerable<AreaDto> GetAreas()
		{
			return _dbContext.Areas.Where(x => User.IsAdmin() || x.AreaUsers.Any(y => y.UserId == User.GetId()))
				.Select(a => new AreaDto { Name = a.Name, Id = a.Id }).ToList();
		}

		[HttpGet("areas/{id}")]
		public ActionResult GetArea(long id)
		{
			AreaDto area = _dbContext.Areas.Where(a => User.IsAdmin() || a.AreaUsers.Any(y => y.UserId == User.GetId()))
				.Where(a => a.Id == id)
				.Select(a => new AreaDto { Name = a.Name, Id = a.Id }).FirstOrDefault();

            if (area == null)
            {
                return NotFound();
            }

            return Ok(area);
		}

		[HttpGet("roles")]
		public IEnumerable<string> GetRoles()
		{
			var roles = User.FindAll("role").Select(x => x.Value);
			return roles;
		}
	}
}