using KnowledgeBank.Multitenant.Abstract;
using System;
using System.Threading.Tasks;
using Microsoft.Azure.SqlDatabase.ElasticScale.ShardManagement;
using Microsoft.Extensions.Options;
using System.Data.SqlClient;

namespace KnowledgeBank.Multitenant
{
	public class TenantSqlConnectionProvider<TTenantIdentity> : ITenantSqlConnectionProvider<TTenantIdentity> where TTenantIdentity : IEquatable<TTenantIdentity>
	{
		private readonly ListShardMap<TTenantIdentity> _shardMap;
		private readonly ShardingConfiguration _config;

		public TenantSqlConnectionProvider(ListShardMap<TTenantIdentity> shardMap, IOptions<ShardingConfiguration> options)
		{
			_shardMap = shardMap;
			_config = options.Value;
		}

		public SqlConnection GetConnection(TTenantIdentity tenant)
		{
			var connection = _shardMap.OpenConnectionForKey(tenant, _config.ConnectionCredentials);

			// Set TenantId in SESSION_CONTEXT to shardingKey to enable Row-Level Security filtering
			using (SqlCommand cmd = connection.CreateCommand())
			{
				cmd.CommandText = @"exec sp_set_session_context @key=N'TenantId', @value=@shardingKey";
				cmd.Parameters.AddWithValue("@shardingKey", tenant);
				cmd.ExecuteNonQuery();
			}

			return connection;
		}
	}
}
