using KnowledgeBank.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System;

namespace KnowledgeBank.Web
{
	/// <summary>
	/// Required for migration scaffolding
	/// </summary>
	public class MigrationFactory : IDbContextFactory<ApplicationDbContext>
	{
		public ApplicationDbContext Create(DbContextFactoryOptions options)
		{
			var builder = new DbContextOptionsBuilder<ApplicationDbContext>();
			builder.UseSqlServer("https://github.com/aspnet/EntityFramework/issues/8427");
			return new ApplicationDbContext(default(long), null, builder.Options);
		}
	}
}

