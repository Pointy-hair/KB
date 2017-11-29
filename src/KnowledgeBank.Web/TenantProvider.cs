using KnowledgeBank.Multitenant;
using Microsoft.AspNetCore.Http;
using System;

namespace KnowledgeBank.Web
{
	public class TenantProvider : ITenantProvider<long>
	{
		private readonly HttpContext _context;

		public TenantProvider(IHttpContextAccessor accessor) => _context = accessor.HttpContext;

		public long GetTenant()
		{
			if (!(_context.Items["tenant"] is long tenantId))
				throw new ArgumentException("tenant not set");
			return tenantId;
		}
	}
}
