using System;
using System.ComponentModel.DataAnnotations;

namespace KnowledgeBank.Web.Models.AreaViewModels
{
	public class AreaViewModel
	{
		public long Id { get; set; }

		[Required]
		public string Name { get; set; }
	}
}
