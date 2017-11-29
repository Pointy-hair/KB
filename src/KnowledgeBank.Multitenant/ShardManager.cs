using Microsoft.Azure.SqlDatabase.ElasticScale.ShardManagement;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace KnowledgeBank.Multitenant
{
	public class ShardManager<T> where T: IEquatable<T>
	{
		private readonly ListShardMap<T> _shardMap;
		private readonly ShardingConfiguration _shardingConfig;
		private readonly object _tenantLock = new object();
		private readonly object _shardLock = new object();

		public ShardManager(ListShardMap<T> shardMap, IOptions<ShardingConfiguration> options)
		{
			_shardMap = shardMap;
			_shardingConfig = options.Value;
		}

		public bool IsEmpty => !_shardMap.GetShards().Any();

		public bool TryAddTenant(T tenantId, string shardName)
		{
			var database = $"{_shardingConfig.ShardPrefix}.{shardName}";
			var location = new ShardLocation(_shardingConfig.ShardServer, database);
			if (!_shardMap.TryGetMappingForKey(tenantId, out _) && _shardMap.TryGetShard(location, out _))
			{
				lock (_tenantLock)
				{
					if (!_shardMap.TryGetMappingForKey(tenantId, out _) && _shardMap.TryGetShard(location, out Shard shard))
					{
						_shardMap.CreatePointMapping(new PointMappingCreationInfo<T>(tenantId, shard, MappingStatus.Online));
						return true;
					}
				}
			}
			return false;
		}

		public bool TryAddShard(string shardName)
		{
			var database = $"{_shardingConfig.ShardPrefix}.{shardName}";
			var location = new ShardLocation(_shardingConfig.ShardServer, database);

			if (!_shardMap.TryGetShard(location, out _))
			{
				lock (_shardLock)
				{
					if (!_shardMap.TryGetShard(location, out _))
					{
						CreateShardDatabase(database);
						_shardMap.CreateShard(location);
						return true;
					}
				}
			}
			return false;
		}

        public IEnumerable<string> GetAllShards()
        {
            string prefix = $"{_shardingConfig.ShardPrefix}.";

            return _shardMap.GetShards().Select(s => 
                s.Location.Database.Split(new string[] { prefix },
                                          StringSplitOptions.RemoveEmptyEntries).FirstOrDefault());
        }

		private void CreateShardDatabase(string shardName)
		{
			using (var sqlConnection = new SqlConnection(_shardingConfig.ServerConnection))
			{
				sqlConnection.Open();
				CreateDatabase(sqlConnection, shardName);
				sqlConnection.ChangeDatabase(shardName);
				using (var transaction = sqlConnection.BeginTransaction())
				{
					try
					{
						CreateSecuritySchema(sqlConnection, transaction);
						CreateSecurityPredicate(sqlConnection, transaction);
						CreateSecurityPolicy(sqlConnection, transaction);
						transaction.Commit();
					}
					catch
					{
						transaction.Rollback();
						DropDatabase(sqlConnection, shardName);
						throw;
					}
				}
			}
		}

		protected virtual void DropDatabase(SqlConnection connection, string name)
		{
			var text = $"DROP Database [{name}];";
			using (var command = new SqlCommand(text, connection))
			{
				command.CommandTimeout = 1000;
				command.ExecuteNonQuery();
			}
		}

		protected virtual void CreateDatabase(SqlConnection connection, string name)
		{
			var serviceObjective = !string.IsNullOrWhiteSpace(_shardingConfig.ElasticPool) ? $"( SERVICE_OBJECTIVE = ELASTIC_POOL ( name = {_shardingConfig.ElasticPool} ) )" : "";
			var text = $"Create Database [{name}] {serviceObjective};";
			using (var command = new SqlCommand(text, connection))
			{
				command.CommandTimeout = 1000;
				command.ExecuteNonQuery();
			}
		}

		protected virtual void CreateSecuritySchema(SqlConnection connection, SqlTransaction transaction)
		{
			var text = "CREATE SCHEMA rls";
			using (var command = new SqlCommand(text, connection, transaction))
				command.ExecuteNonQuery();
		}


		protected virtual SqlDbType GetSqlType()
		{
			if (typeof(T) == typeof(int))
				return SqlDbType.Int;

			if (typeof(T) == typeof(Guid))
				return SqlDbType.UniqueIdentifier;

			if (typeof(T) == typeof(string))
				return SqlDbType.NVarChar;

			if (typeof(T) == typeof(long))
				return SqlDbType.BigInt;

			if (typeof(T) == typeof(short))
				return SqlDbType.SmallInt;

			return SqlDbType.Binary;
		}

		protected virtual void CreateSecurityPredicate(SqlConnection connection, SqlTransaction transaction)
		{
			var principal = String.IsNullOrWhiteSpace(_shardingConfig.DatabasePrincipal) ? "dbo" : _shardingConfig.DatabasePrincipal;
			var superUser = String.IsNullOrWhiteSpace(_shardingConfig.DatabaseSuperUser) ? "superuser" : _shardingConfig.DatabaseSuperUser;

			var type = GetSqlType();

			var text = $@"CREATE FUNCTION rls.fn_tenantAccessPredicate(@TenantId {type})
	RETURNS TABLE
	WITH SCHEMABINDING
AS
	RETURN SELECT 1 AS fn_accessResult
		WHERE 
		(
			DATABASE_PRINCIPAL_ID() = DATABASE_PRINCIPAL_ID('{principal}') 
			AND CAST(SESSION_CONTEXT(N'TenantId') AS {type}) = @TenantId
		)
		OR
		(
			DATABASE_PRINCIPAL_ID() = DATABASE_PRINCIPAL_ID('{superUser}')
		);";
			using (var command = new SqlCommand(text, connection, transaction))
				command.ExecuteNonQuery();
		}

		protected virtual void CreateSecurityPolicy(SqlConnection connection, SqlTransaction transaction)
		{
			var text = "CREATE SECURITY POLICY rls.tenantAccessPolicy WITH(STATE = ON)";
			using (var command = new SqlCommand(text, connection, transaction))
				command.ExecuteNonQuery();
		}
	}
}
