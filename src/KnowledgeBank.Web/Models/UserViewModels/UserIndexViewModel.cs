using System.Collections.Generic;

namespace KnowledgeBank.Web.Models.UserViewModels
{
	public class UserIndexViewModel
	{
		public string Id { get; set; }
		public string UserName { get; set; }
		public string Email { get; set; }
		public List<string> Areas { get; set; }
	}
}
