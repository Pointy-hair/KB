using IdentityServer4.EntityFramework.DbContexts;
using IdentityServer4.EntityFramework.Mappers;
using KnowledgeBank.Domain;
using KnowledgeBank.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace KnowledgeBank.Persistence
{
	public static class DbInitializer
	{
		public static void InitializeConfigStore(ConfigurationDbContext context, string jsHost)
		{
			if (!context.Clients.Any())
			{
				foreach (var client in Config.GetClients(jsHost))
				{
					context.Clients.Add(client.ToEntity());
				}
				context.SaveChanges();
			}

			if (!context.IdentityResources.Any())
			{
				foreach (var resource in Config.GetIdentityResources())
				{
					context.IdentityResources.Add(resource.ToEntity());
				}
				context.SaveChanges();
			}

			if (!context.ApiResources.Any())
			{
				foreach (var resource in Config.GetApiResources())
				{
					context.ApiResources.Add(resource.ToEntity());
				}
				context.SaveChanges();
			}
		}

		public static void Initialize(ApplicationIdentityDbContext context, IServiceProvider serviceProvider)
		{
			using (var transaction = context.Database.BeginTransaction())
			{
				try
				{
					SeedRoles(context, serviceProvider).Wait();
					SeedRootAdmin(context, serviceProvider).Wait();

					transaction.Commit();
				}
				catch (Exception e)
				{
					File.WriteAllText($"log{DateTime.UtcNow.Date.ToString("yyyyMMdd")}.txt", e.ToString());
				}
			}
		}

		public static async Task<ApplicationUser> SeedRootAdmin(ApplicationIdentityDbContext context, IServiceProvider serviceProvider)
		{
			const string ROOT_ADMIN_EMAIL = "owner@dummy.com";
			var owner = context.Users.FirstOrDefault(user => user.UserName == ROOT_ADMIN_EMAIL);
			if (owner is null)
			{
				var userManager = serviceProvider.GetService<UserManager<ApplicationUser>>();
				owner = new ApplicationUser()
				{
					Email = ROOT_ADMIN_EMAIL,
					NormalizedEmail = ROOT_ADMIN_EMAIL.ToUpper(),
					UserName = ROOT_ADMIN_EMAIL,
					NormalizedUserName = ROOT_ADMIN_EMAIL.ToUpper(),
					PhoneNumber = "+123456789",
					EmailConfirmed = true,
					PhoneNumberConfirmed = true,
				};

				await userManager.CreateAsync(owner, "`12QWEasd"); // user must be warned to change this
				context.SaveChanges();

				await userManager.AddToRoleAsync(owner, Role.Admin);
				context.SaveChanges();
			}
			return owner;
		}

		public static async Task<List<IdentityRole>> SeedRoles(ApplicationIdentityDbContext context, IServiceProvider serviceProvider)
		{
			var roles = context.Roles.ToList();

			if (!roles.Any())
			{
				var roleStore = serviceProvider.GetService<RoleManager<IdentityRole>>();
				roles = new List<IdentityRole>
				{
					new IdentityRole(Role.Admin) { Id = Constants.AdminRoleId },
					new IdentityRole(Role.AreaAdmin) { Id = Constants.AreaAdminRoleId },
					new IdentityRole(Role.ReadOnlyUser) { Id = Constants.ReadOnlyUserRoleId },
					new IdentityRole(Role.ReadWriteUser) { Id = Constants.ReadWriteUserRoleId }
				};
				foreach (var role in roles)
					await roleStore.CreateAsync(role);
			}
			return roles;
		}
	}
}
