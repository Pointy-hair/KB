using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using System;
using System.Data.Common;
using System.Data;
using Microsoft.Extensions.Logging;

namespace KnowledgeBank.Multitenant.Abstract
{
	public class TenantDbContext<TTenantIdentity> : DbContext where TTenantIdentity : IEquatable<TTenantIdentity>
	{
		public TTenantIdentity TenantId { get; set; }

		private readonly ILogger<TenantDbContext<TTenantIdentity>> _logger;

		public TenantDbContext(TTenantIdentity tenantId, ILoggerFactory loggerFactory, DbContextOptions options) : base(options)
		{
			_logger = loggerFactory?.CreateLogger<TenantDbContext<TTenantIdentity>>();
			TenantId = tenantId;
		}

		public override int SaveChanges()
		{
			SetTenant();
			return base.SaveChanges();
		}

		public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default(CancellationToken))
		{
			SetTenant();
			return base.SaveChangesAsync(cancellationToken);
		}

		private void SetTenant()
		{
			foreach (var item in ChangeTracker.Entries().Where(e => e.State == EntityState.Added))
				if (item.Metadata.FindProperty("TenantId") != null)
					item.CurrentValues["TenantId"] = TenantId;
		}

		public override void Dispose() => Dispose(true);

		public void Dispose(bool closeConnection)
		{
			if(closeConnection)
			{
				try
				{
					DbConnection dbConnection = this.Database.GetDbConnection();
					if (dbConnection.State != ConnectionState.Closed)
						dbConnection.Close();
				}
				catch (Exception ex)
				{
					_logger?.LogError(eventId: 150, exception: ex, message: "could not close connection");
				}
			}
			
			base.Dispose();
		}

	}
}