using KnowledgeBank.Domain;
using KnowledgeBank.Multitenant.Abstract;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;

namespace KnowledgeBank.Persistence
{
	/// <summary>
	/// TODO change to application specific dbContext
	/// </summary>
	public class ApplicationDbContext : TenantDbContext<long>
	{

		public ApplicationDbContext(long tenantId, ILoggerFactory loggerFactory, DbContextOptions<ApplicationDbContext> options) : base(tenantId, loggerFactory, options)
		{
		}

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
        }

        public DbSet<Case> Cases { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<Step> Steps { get; set; }
        public DbSet<Link> Links { get; set; }
        public DbSet<Attachment> Attachments { get; set; }
    }
}

