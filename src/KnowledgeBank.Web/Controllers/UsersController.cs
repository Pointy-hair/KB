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
	[Authorize]
	[SecurityHeaders]
	[Route("[controller]")]
	[Branch(Branch.Identity)]
	public class UsersController : Controller
	{
		private readonly UserManager<ApplicationUser> _userManager;
		private readonly ApplicationIdentityDbContext _context;
		private readonly IEmailSender _emailSender;
		private readonly ISmsSender _smsSender;
		private readonly ILogger _logger;

		public UsersController(
			UserManager<ApplicationUser> userManager,
			ApplicationIdentityDbContext context,
			IEmailSender emailSender,
			ISmsSender smsSender,
			ILoggerFactory loggerFactory)
		{
			_userManager = userManager;
			_context = context;
			_emailSender = emailSender;
			_smsSender = smsSender;
			_logger = loggerFactory.CreateLogger<UsersController>();
		}

		[HttpGet]
		[Authorize(Roles = Role.AreaAdmin + ", " + Role.Admin)]
		public async Task<IActionResult> Index()
		{
			var admin = await _userManager.GetUserAsync(User);
			var areas = await GetUserAreasAsync(admin);
			var isRoot = User.IsInRole(Role.Admin);

			var result = await _context.Users
				.Where(x => x.Roles.All(role => role.RoleId != Constants.AdminRoleId))
				.Where(x => isRoot || x.UserAreas.Any(ua => areas.Contains(ua.AreaId)))
				.Select(user => new UserIndexViewModel
				{
					Id = user.Id,
					UserName = user.UserName,
					Email = user.Email,
				})
				.ToArrayAsync();

			return View(result);
		}

		[HttpPost("Delete/{id}")]
		[Authorize(Roles = Role.Admin)]
		public async Task<IActionResult> Delete(string id)
		{
			var user = await _userManager.FindByIdAsync(id);
			if (user is null)
				return BadRequest();
			using(var transaction = await _context.Database.BeginTransactionAsync())
			{
				await _userManager.DeleteAsync(user);
				_context.RemoveRange(_context.UsersAreas.Where(ua => ua.UserId == user.Id));
				await _context.SaveChangesAsync();
				transaction.Commit();
			}
			return RedirectToAction(nameof(Index));
		}

		private List<SelectListItem> BuildAllowedRoleList(ClaimsPrincipal user)
		{
			var roles = new List<SelectListItem>
			{
				new SelectListItem { Value = Role.ReadOnlyUser, Text = "Read-Only User" },
				new SelectListItem { Value = Role.ReadWriteUser, Text = "Read-Write User" },
				new SelectListItem { Value = Role.AreaAdmin, Text = "Area Admin" },
			};
			if (user.IsInRole(Role.Admin))
				roles.Add(new SelectListItem { Value = Role.Admin, Text = Role.Admin });
			return roles;
		}

		private async Task<List<SelectListItem>> BuildAllowedAreaList()
		{
			IQueryable<Area> query = _context.Areas;
			if (!User.IsInRole(Role.Admin))
			{
				var id = _userManager.GetUserId(User);
				var allowedAreas = await _context.UsersAreas.Where(x => x.UserId == id).Select(x => x.AreaId).ToArrayAsync();
				query = query.Where(x => allowedAreas.Contains(x.Id));
			}

			return await query.Select(x => new SelectListItem
			{
				Value = x.Id.ToString(),
				Text = x.Name
			}).ToListAsync();
		}

		private async Task<long[]> GetUserAreasAsync(ApplicationUser user)
		{
			var claims = await _userManager.GetClaimsAsync(user);
			return claims.Where(x => x.Type == "tenant").Select(x => long.Parse(x.Value)).ToArray();
		}

		private async Task<bool> UserBelongsToAdminAreas(ApplicationUser user)
		{
			if (!User.IsInRole(Role.Admin))
			{
				var areaAdmin = await _userManager.GetUserAsync(User);

				var adminTenants = await GetUserAreasAsync(areaAdmin);
				var userTenants = await GetUserAreasAsync(user);

				if (!adminTenants.Intersect(userTenants).Any())
					return false;
			}
			return true;
		}

		[HttpGet("Edit/{id}")]
		[Authorize(Roles = Role.AreaAdmin + ", " + Role.Admin)]
		public async Task<IActionResult> Edit(string id)
		{
			var user = await _userManager.FindByIdAsync(id);
			if (!User.IsInRole(Role.Admin))
			{
				var areaAdmin = await _userManager.GetUserAsync(User);

				var adminTenants = await GetUserAreasAsync(areaAdmin);
				var userTenants = await GetUserAreasAsync(user);

				if (!adminTenants.Intersect(userTenants).Any())
					return BadRequest();
			}

			var roles = await _userManager.GetRolesAsync(user);
			var viewModel = new UserEditViewModel
			{
				Email = user.Email,
				UserName = user.UserName,
				Role = roles.FirstOrDefault() ?? Role.ReadOnlyUser,
				Roles = BuildAllowedRoleList(User),
				SelectedAreas = _context.UsersAreas.Where(x => x.UserId == user.Id).Select(x => x.AreaId).ToArray(),
				Areas = await BuildAllowedAreaList(),
			};
			return View(viewModel);
		}


		[HttpPost("Edit/{id}")]
		[Authorize(Roles = Role.AreaAdmin + ", " + Role.Admin)]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Edit(string id, UserEditViewModel model)
		{
			ApplicationUser user = await _userManager.FindByIdAsync(id);

			if (!await UserBelongsToAdminAreas(user))
				return BadRequest();

			using (var transaction = await _context.Database.BeginTransactionAsync())
			{

				if (model.Email != user.Email)
					HandleIfErrors(await _userManager.SetEmailAsync(user, model.Email));
				if (model.UserName != user.UserName)
					HandleIfErrors(await _userManager.SetUserNameAsync(user, model.UserName));

				var role = (await _userManager.GetRolesAsync(user)).First();
				if (role != model.Role)
				{
					HandleIfErrors(await _userManager.RemoveFromRoleAsync(user, role));
					HandleIfErrors(await _userManager.AddToRoleAsync(user, model.Role));
				}

				var userAreas = await GetUserAreasAsync(user);

				var admin = await _userManager.GetUserAsync(User);
				var adminAreas = await GetUserAreasAsync(admin);
				bool isAreaAdmin = User.IsInRole(Role.AreaAdmin);

				// A = admin areas
				// S = selected areas
				// D = user areas from database

				// S / (A u D)
				if (isAreaAdmin)
				{
					var illegal = model.SelectedAreas.Except(userAreas.Union(adminAreas));

					if (illegal.Any())
						ModelState.AddModelError("No Claims", "No claims for all of the selected areas");
				}

				// (S / D) ∩ A
				var toAdd = (model.SelectedAreas.Except(userAreas));
				if (isAreaAdmin)
					toAdd = toAdd.Intersect(adminAreas);

				// (D / S) ∩ A
				var toRemove = (userAreas.Except(model.SelectedAreas));
				if (isAreaAdmin)
					toRemove = toRemove.Intersect(adminAreas);

				var removeList = toRemove.ToList();
				if (removeList.Count > 0)
				{
					HandleIfErrors(await _userManager.RemoveClaimsAsync(user, removeList.Select(x => new Claim("tenant", x.ToString()))));
					_context.UsersAreas.RemoveRange(_context.UsersAreas.Where(x => removeList.Contains(x.AreaId) && x.UserId == user.Id));
				}

				var addList = toAdd.ToList();
				if (addList.Count > 0)
				{
					HandleIfErrors(await _userManager.AddClaimsAsync(user, addList.Select(x => new Claim("tenant", x.ToString()))));
					await _context.UsersAreas.AddRangeAsync(addList.Select(areaId => new UserArea { UserId = user.Id, AreaId = areaId }));
				}

				await _context.SaveChangesAsync();

				if (ModelState.IsValid)
				{
					transaction.Commit();
					return RedirectToAction(nameof(Index));
				}
				else
				{
					transaction.Rollback();
					model.Areas = await BuildAllowedAreaList();
					model.Roles = BuildAllowedRoleList(User);
					return View(model);
				}
			}
		}


		[HttpGet("Create")]
		[Authorize(Roles = Role.AreaAdmin + ", " + Role.Admin)]
		public async Task<IActionResult> Create(string returnUrl = null)
		{
			ViewData["ReturnUrl"] = returnUrl;

			var viewModel = new RegisterViewModel
			{
				Areas = await BuildAllowedAreaList(),
				Roles = BuildAllowedRoleList(User)
			};
			return View(viewModel);
		}

		[HttpPost("Create")]
		[Authorize(Roles = Role.AreaAdmin + ", " + Role.Admin)]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Create(RegisterViewModel model, string returnUrl = null)
		{
			await ValidateAsync(model);

			ViewData["ReturnUrl"] = returnUrl;
			if (ModelState.IsValid)
			{
				var user = new ApplicationUser
				{
					UserName = model.UserName,
					Email = model.Email,
					UserAreas = model.SelectedAreas.Select(x => new UserArea { AreaId = x }).ToList(),
					EmailConfirmed = false
				};

				var result = await _userManager.CreateAsync(user, model.Password);
				if (result.Succeeded)
				{
					await _userManager.AddClaimsAsync(user, model.SelectedAreas.Select(x => new Claim("tenant", x.ToString())));
					await _userManager.AddToRoleAsync(user, model.Role);

					// For more information on how to enable account confirmation and password reset please visit http://go.microsoft.com/fwlink/?LinkID=532713
					// Send an email with this link
					var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
					var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, code = code }, protocol: HttpContext.Request.Scheme);
					await _emailSender.SendEmailAsync(model.UserName, model.Email, "Confirm your account",
						$"Please confirm your account by clicking this link: <a href='{callbackUrl}'>link</a>");
					_logger.LogInformation(3, "User created.");
					return RedirectToLocal(returnUrl);
				}
				HandleIfErrors(result);
			}

			model.Areas = await BuildAllowedAreaList();
			model.Roles = BuildAllowedRoleList(User);
			// If we got this far, something failed, redisplay form
			return View(model);
		}

		private async Task ValidateAsync(UserViewModelBase model)
		{
			if (!User.IsInRole(Role.Admin))
			{
				var id = _userManager.GetUserId(User);
				var ids = model.SelectedAreas.ToArray();
				var hasClaimsOnSelectedAreas = await _context.UsersAreas.Where(x => x.UserId == id).AllAsync(x => ids.Contains(x.AreaId));
				if (!hasClaimsOnSelectedAreas)
					ModelState.AddModelError("No Claims", "No claims on selected Areas");
				var possibleRoles = new List<string> { Role.AreaAdmin, Role.ReadOnlyUser, Role.ReadWriteUser };
				if (!possibleRoles.Contains(model.Role))
					ModelState.AddModelError("Not authorized", "Not authorized for selected role");
			}
		}

		private bool HandleIfErrors(IdentityResult result)
		{
			foreach (var error in result.Errors)
				ModelState.AddModelError(error.Code, error.Description);
			return !result.Succeeded;
		}


		private IActionResult RedirectToLocal(string returnUrl)
		{
			if (Url.IsLocalUrl(returnUrl))
				return Redirect(returnUrl);
			else
				return Redirect("~/");
		}
	}
}
