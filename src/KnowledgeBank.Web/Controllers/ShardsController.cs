using IdentityServer4.Quickstart.UI;
using KnowledgeBank.Domain;
using KnowledgeBank.Multitenant;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using KnowledgeBank.Web.Helpers;

namespace KnowledgeBank.Web.Controllers
{
	[Authorize(Roles = Role.Admin)]
	[Branch(Branch.Identity)]
	public class ShardsController : Controller
	{
		private readonly ShardManager<long> _shardManager;

		public ShardsController(ShardManager<long> shardManager)
		{
			this._shardManager = shardManager;
		}

		[HttpGet]
		public IActionResult Index()
		{
			IEnumerable<string> existingShards = _shardManager.GetAllShards();
			return View(existingShards);
		}

		[HttpPost]
		public IActionResult Index(string newShardName)
		{
			if (string.IsNullOrWhiteSpace(newShardName))
			{
				ModelState.AddModelError(nameof(newShardName), "You must provide a shard name");
			}
			IEnumerable<string> existingShards = _shardManager.GetAllShards();
			if (existingShards.Contains(newShardName))
			{
				ModelState.AddModelError(nameof(newShardName), "A shard with the same name already exists");
			}

			if (!_shardManager.TryAddShard(newShardName))
			{
				ModelState.AddModelError(string.Empty, "Shard not saved (internal error)");
			}

			return Index();
		}
	}
}
