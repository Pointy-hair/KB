using IdentityModel;
using KnowledgeBank.Domain;
using System.Security.Claims;

namespace KnowledgeBank.Web.Extensions
{
    public static class RoleCheckingExtensions
    {
        public static string GetId(this ClaimsPrincipal user)
        {
            return user.FindFirst(JwtClaimTypes.Subject).Value;
        }

        public static bool IsAdmin(this ClaimsPrincipal user)
        {
            return user.IsInRole(Role.Admin);
        }
    }
}
