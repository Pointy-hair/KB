using IdentityServer4;
using IdentityServer4.Models;
using System.Collections.Generic;

namespace KnowledgeBank.Persistence
{
	public class Config
	{
		// scopes define the API resources in your system
		public static IEnumerable<IdentityResource> GetIdentityResources()
		{
			return new List<IdentityResource>
			{
				new IdentityResources.OpenId(),
				new IdentityResources.Profile(),
			};
		}

		public static IEnumerable<ApiResource> GetApiResources()
		{
			return new List<ApiResource>
			{
				new ApiResource("webapi", "My API")
				{
					UserClaims = new List<string>{ "tenant", "role" }
				}
			};
		}

		// client want to access resources (aka scopes)
		public static IEnumerable<Client> GetClients(string jsHost)
		{
			return new List<Client>
			{
				// JavaScript Client
				new Client
				{
					ClientId = "js",
					ClientName = "JavaScript Client",
					AllowedGrantTypes = GrantTypes.Implicit,
					AllowAccessTokensViaBrowser = true,
					RequireConsent = false,

					RedirectUris = { $"{jsHost}/auth-callback.html", $"{jsHost}/silent-renew.html" },
					PostLogoutRedirectUris = { jsHost },
					AllowedCorsOrigins = { jsHost },

					AllowedScopes =
					{
						IdentityServerConstants.StandardScopes.OpenId,
						IdentityServerConstants.StandardScopes.Profile,
						"webapi"
					}
				}
			};
		}
	}
}
