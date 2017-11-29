using System;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace KnowledgeBank.Multitenant.Abstract
{
	public interface ITenantSqlConnectionProvider<TTenantIdentity> where TTenantIdentity : IEquatable<TTenantIdentity>
	{
		SqlConnection GetConnection(TTenantIdentity tenant);
	}
}
