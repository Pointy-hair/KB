using KnowledgeBank.Web.Models.AreaViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using KnowledgeBank.Multitenant;
using System.Linq;
using System;
using KnowledgeBank.Domain;
using KnowledgeBank.Persistence;
using IdentityServer4.Quickstart.UI;
using KnowledgeBank.Web.Helpers;

namespace KnowledgeBank.Web.Controllers
{
	[Authorize(Roles = Role.Admin)]
	[Branch(Branch.Identity)]
	public class AreasController : Controller
	{
		private readonly ApplicationIdentityDbContext _dbContext;
		private readonly UserManager<ApplicationUser> _userManager;
		private readonly ShardManager<long> _shardManager;

		public AreasController(ApplicationIdentityDbContext dbContext, UserManager<ApplicationUser> userManager, ShardManager<long> shardManager)
		{
			this._dbContext = dbContext;
			this._userManager = userManager;
			this._shardManager = shardManager;
		}

		[HttpGet]
		public IActionResult Manage()
		{
			var existingShards = _shardManager.GetAllShards();
			var tenants = _dbContext.Areas.Select(t => new AreaViewModel { Id = t.Id, Name = t.Name }).ToList();
			var vm = new ManageAreasViewModel
			{
				Areas = tenants,
				Shards = existingShards
			};
			return View(vm);
		}

		[HttpPost]
		public IActionResult Manage(string area, string shard)
		{
			ValidateAreaName(area);

			var existingShards = _shardManager.GetAllShards();
			if (!existingShards.Contains(shard))
			{
				ModelState.AddModelError(nameof(shard), "Invalid shard selection");
			}

			if (!ModelState.IsValid)
			{
				return Manage();
			}

			var newArea = new Area { Name = area };
			_dbContext.Areas.Add(newArea);
			_dbContext.SaveChanges();

			_shardManager.TryAddTenant(newArea.Id, shard);

			// TODO: add Tenant admin in the same round to avoid sitiations where a tenant has no admin

			return Manage();
		}

		private void ValidateAreaName(string area)
		{
			if (string.IsNullOrWhiteSpace(area))
			{
				ModelState.AddModelError(nameof(area), "Area name not provided");
			}

			var existingTenants = _dbContext.Areas;
			if (existingTenants.Any(t => t.Name == area))
			{
				ModelState.AddModelError(nameof(area), "An area with the same name already exists");
			}
		}

		[HttpPost]
		public IActionResult Delete([FromRoute] long id)
		{
			// TODO: what should we delete exactly?
			Console.WriteLine($"kagbă delete: {id}");
			return RedirectToAction(nameof(this.Manage));
		}

		[HttpPost]
		public IActionResult Update([FromRoute] long id, string newName)
		{
			ValidateAreaName(newName);
			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}

			var editedArea = _dbContext.Areas.SingleOrDefault(area => area.Id == id);


			_dbContext.Areas.Update(editedArea);
			editedArea.Name = newName;
			_dbContext.SaveChanges();

			return RedirectToAction(nameof(this.Manage));
		}
	}
}
