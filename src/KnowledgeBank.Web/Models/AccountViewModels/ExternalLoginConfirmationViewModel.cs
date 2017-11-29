using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace KnowledgeBank.Web.Models.AccountViewModels
{
	public class ExternalLoginConfirmationViewModel
	{
		[Required]
		public string UserName { get; set; }
	}
}
