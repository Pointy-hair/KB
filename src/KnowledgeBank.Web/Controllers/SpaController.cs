using System;
using KnowledgeBank.Domain;
using KnowledgeBank.Persistence;
using KnowledgeBank.Web.Models.AccountViewModels;
using KnowledgeBank.Web.Models.UserViewModels;
using KnowledgeBank.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using IdentityServer4.Quickstart.UI;
using KnowledgeBank.Web.Helpers;

namespace KnowledgeBank.Web.Controllers
{
	public class SpaController : ControllerBase
	{
		public VirtualFileResult Index() => File("index.html", "text/html");
	}
}