using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace KnowledgeBank.Domain
{ 
	public class Area 
	{
		public long Id { get; set; }
		[Required]
		[MaxLength(200)]
		public string Name { get; set; }
		public ICollection<UserArea> AreaUsers { get; set; }
	}
}
