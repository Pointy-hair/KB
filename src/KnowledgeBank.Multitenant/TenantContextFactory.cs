using KnowledgeBank.Multitenant.Abstract;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;

namespace KnowledgeBank.Multitenant
{
	public class TenantContextFactory<TContext, TTenantIdentity> where TContext : TenantDbContext<TTenantIdentity> where TTenantIdentity : IEquatable<TTenantIdentity>
	{
		protected readonly ConcurrentDictionary<TTenantIdentity, Lazy<DbContextOptions<TContext>>> optionsCache = new ConcurrentDictionary<TTenantIdentity, Lazy<DbContextOptions<TContext>>>();
		protected readonly Action<TTenantIdentity, DbContextOptionsBuilder<TContext>> globalOptionsActions;
		protected readonly Func<TTenantIdentity, ILoggerFactory, DbContextOptions<TContext>, TContext> factory;

		public TenantContextFactory(
			Func<TTenantIdentity, ILoggerFactory, DbContextOptions<TContext>, TContext> factory,
			Action<TTenantIdentity, DbContextOptionsBuilder<TContext>> optionsAction = null)
		{
			this.factory = factory;
			this.globalOptionsActions = optionsAction;
		}

		public TContext Create(TTenantIdentity tenant, ILoggerFactory loggerFactory, Action<DbContextOptionsBuilder<TContext>> providerOptionsAction)
		{
			DbContextOptions<TContext> cachedOptions = GetOrAddOptions(tenant, loggerFactory, providerOptionsAction);
			var opts = new DbContextOptionsBuilder<TContext>(cachedOptions);
			providerOptionsAction?.Invoke(opts);
			return factory(tenant, loggerFactory, opts.Options);
		}

		protected virtual DbContextOptions<TContext> GetOrAddOptions(
			TTenantIdentity tenant,
			ILoggerFactory loggerFactory,
			Action<DbContextOptionsBuilder<TContext>> providerOptionsAction)
		{
			var cachedOptions = this.optionsCache.GetOrAdd(
				tenant,
				t => new Lazy<DbContextOptions<TContext>>(() =>
				{
					var optionBuilder = new DbContextOptionsBuilder<TContext>();
					globalOptionsActions?.Invoke(t, optionBuilder);

					Migrate(t, loggerFactory, providerOptionsAction, optionBuilder.Options);

					return optionBuilder.Options;
				}));
			return cachedOptions.Value;
		}

		private void Migrate(TTenantIdentity t, ILoggerFactory loggerFactory, Action<DbContextOptionsBuilder<TContext>> providerOptionsAction, DbContextOptions<TContext> options)
		{
			var migrationOptions = new DbContextOptionsBuilder<TContext>(options);
			providerOptionsAction?.Invoke(migrationOptions);

			TContext context = null;
			try
			{
				context = factory(t, loggerFactory, migrationOptions.Options);
				context.Database.Migrate();
			}
			finally
			{
				context?.Dispose(closeConnection: false);
			}

		}
	}
}
