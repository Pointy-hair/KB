using System;

namespace KnowledgeBank.Domain
{
	public class UserArea
	{
		public string UserId { get; set; }
		public ApplicationUser User { get; set; }

		public long AreaId { get; set; }
		public Area Area { get; set; }
	}
}
