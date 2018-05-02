using System.Linq;
using System.Security.Claims;
using AuthN.Configuration;
using Nancy;

namespace AuthN.Modules.Extensions {
    public static class ModuleSecurityExtensions {
        public static void assertClaims(this NancyModule module, ISContext context,
            params string[] requiredClaims) {
            module.Before.AddItemToEndOfPipeline(ctx => {
                if (ctx.CurrentUser == null) return HttpStatusCode.Unauthorized;

                foreach (var requiredClaim in requiredClaims) {
                    if (!ctx.CurrentUser.HasClaim(x => x.Type == requiredClaim)) {
                        return HttpStatusCode.Unauthorized; // missing claims
                    }
                }

                return null; // continue, granted.
            });
        }

        public static string getClaim(this ClaimsPrincipal identity, string type) {
            return identity.Claims.FirstOrDefault(x => x.Type == type)?.Value;
        }
    }
}