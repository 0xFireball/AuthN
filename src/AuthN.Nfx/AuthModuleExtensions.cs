using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Nancy;

namespace AuthN.Nfx {
    public static class AuthModuleExtensions {
        public static void assertClaims(this NancyModule module,
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

        public static IDictionary<string, object> getContextClaims(this NancyContext context) {
            return (IDictionary<string, object>) context.Items["claims"];
        }
    }
}