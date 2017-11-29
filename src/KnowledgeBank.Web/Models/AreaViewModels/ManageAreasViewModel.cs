using System.Collections.Generic;

namespace KnowledgeBank.Web.Models.AreaViewModels
{
	public class ManageAreasViewModel
	{
		public List<AreaViewModel> Areas { get; set; }
		public string Area { get; set; }
		public IEnumerable<string> Shards { get; set; }
		public string Shard { get; set; }
	}
}
