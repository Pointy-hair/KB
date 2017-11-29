using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace KnowledgeBank.Web.Models.UserViewModels
{
	public class UserViewModelBase
	{
		public UserViewModelBase()
		{
			SelectedAreas = new long[0];
		}

		public string Id { get; set; }

		[Required]
		[MaxLength(16)]
		[Display(Name = "Username")]
		public string UserName { get; set; }

		[Required]
		[EmailAddress]
		[Display(Name = "Email")]
		public string Email { get; set; }

		[Display(Name = "Areas")]
		public long[] SelectedAreas { get; set; }

		[Required(ErrorMessage = "Role should be selected")]
		public string Role { get; set; }
		public List<SelectListItem> Roles { get; set; }
		public List<SelectListItem> Areas { get; set; }
	}
}