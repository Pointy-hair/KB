using System;

namespace KnowledgeBank.Multitenant
{
	public interface ITenantProvider<out TTenantIdentity> where TTenantIdentity : IEquatable<TTenantIdentity>
	{
		TTenantIdentity GetTenant();
	}
}
