using Microsoft.EntityFrameworkCore;
using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Azure.SqlDatabase.ElasticScale.ShardManagement;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.DependencyInjection.Extensions;
using KnowledgeBank.Multitenant.Abstract;
using Microsoft.Extensions.Logging;
using System.Data.SqlClient;

namespace KnowledgeBank.Multitenant
{
	public static class TenantExtensions
	{
		private static void EnsureDbExists(string connectionstring, string elasticPool)
		{
			SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder(connectionstring);
			var database = builder.InitialCatalog;
			builder.Remove("Initial Catalog");

			using (var connection = new SqlConnection(builder.ToString()))
			{
				connection.Open();

				var serviceObjective = !string.IsNullOrWhiteSpace(elasticPool) ? $"( SERVICE_OBJECTIVE = ELASTIC_POOL ( name = {elasticPool} ) )" : "";
				var createText = $"Create Database [{database}] {serviceObjective};";

				using (var command = new SqlCommand(createText, connection))
				{
					command.CommandText = $@"IF DB_ID('{database}') IS NULL
					BEGIN
						{createText}
					END";
					command.CommandTimeout = 1000;
					command.ExecuteNonQuery();
				}
			}
		}

		private static void TryAddShardMapManagement<TTenantIdentity>(IServiceCollection services,
			string connectionString)
		{
			services.TryAddSingleton(s =>
			{
				var shardingConfig = s.GetService<IOptions<ShardingConfiguration>>().Value;
				EnsureDbExists(connectionString, shardingConfig.ElasticPool);
				if (!ShardMapManagerFactory.TryGetSqlShardMapManager(
					connectionString,
					ShardMapManagerLoadPolicy.Lazy,
					out ShardMapManager shardMapManager))
				{

					ShardMapManagerFactory.CreateSqlShardMapManager(connectionString);

					shardMapManager = ShardMapManagerFactory.GetSqlShardMapManager(
						connectionString,
						ShardMapManagerLoadPolicy.Lazy);
				}

				return shardMapManager;
			});

			services.TryAddSingleton(s =>
			{
				var smm = s.GetService<ShardMapManager>();
				var options = s.GetService<IOptions<ShardingConfiguration>>().Value;
				string mapName = options.ShardMap;
				// check if shardmap exists and if not, create it
				if (!smm.TryGetListShardMap(mapName, out ListShardMap<TTenantIdentity> shardMap))
					return smm.CreateListShardMap<TTenantIdentity>(mapName);
				return shardMap;
			});
		}

		public static void AddShardManager<TTenantIdentity>(this IServiceCollection services,
			string connectionString) where TTenantIdentity : IEquatable<TTenantIdentity>
		{
			TryAddShardMapManagement<TTenantIdentity>(services, connectionString);
			services.TryAddSingleton<ShardManager<TTenantIdentity>>();
		}

		public static void AddTenantContextFactory<TContext, TTenantIdentity, TTenantProvider>(
			this IServiceCollection services,
			string connectionString,
			Func<TTenantIdentity, ILoggerFactory, DbContextOptions<TContext>, TContext> factory,
			Action<TTenantIdentity, DbContextOptionsBuilder<TContext>> optionsAction = null)

			where TContext : TenantDbContext<TTenantIdentity>
			where TTenantIdentity : IEquatable<TTenantIdentity>
			where TTenantProvider : class, ITenantProvider<TTenantIdentity>
		{
			TryAddShardMapManagement<TTenantIdentity>(services, connectionString);
			services.AddSingleton(_ => new TenantContextFactory<TContext, TTenantIdentity>(factory, optionsAction));

			services.AddEntityFrameworkSqlServer();
			services.AddScoped<IMigrationsSqlGenerator, TenantSqlServerMigrationSqlGenerator>();
			services.AddScoped<ITenantProvider<TTenantIdentity>, TTenantProvider>();
			services.AddScoped<ITenantSqlConnectionProvider<TTenantIdentity>, TenantSqlConnectionProvider<TTenantIdentity>>();
			services.AddScoped(x =>
			{
				var tenantProvider = x.GetService<ITenantProvider<TTenantIdentity>>();
				var tenant = tenantProvider.GetTenant();

				var sqlConnectionProvider = x.GetService<ITenantSqlConnectionProvider<TTenantIdentity>>();
				var connection = sqlConnectionProvider.GetConnection(tenant);

				var loggerFactory = x.GetService<ILoggerFactory>();
				var contextFactory = x.GetService<TenantContextFactory<TContext, TTenantIdentity>>();

				return contextFactory.Create(tenant, loggerFactory, b => b.UseSqlServer(connection).UseInternalServiceProvider(x));
			});
		}
	}
}
