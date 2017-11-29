using System;

namespace KnowledgeBank.Multitenant.Abstract
{
	public abstract class TenantBoundEntity<TIdentity, TTenantIdentity> where TIdentity : IEquatable<TIdentity> where TTenantIdentity : IEquatable<TTenantIdentity>
	{
		public TIdentity Id { get; set; }
		public TTenantIdentity TenantId { get; set; }
	}
}
