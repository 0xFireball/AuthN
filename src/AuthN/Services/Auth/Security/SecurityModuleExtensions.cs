using System.Security.Claims;
using Nancy;

namespace AuthN.Services.Auth.Security {
    public static class ApiAccessModuleSecurityExtensions {
        private static Claim _adminClaim =
            new Claim(ApiAuthenticator.AUTH_TYPE_CLAIM_KEY, ApiAccessScope.Admin.ToString());

        private static Claim _userClaim =
            new Claim(ApiAuthenticator.AUTH_TYPE_CLAIM_KEY, ApiAccessScope.User.ToString());

        public static void requiresUserAuthentication(this NancyModule module) {
            injectAuthenticationHook(module, _userClaim);
        }

        public static void requiresAdminAuthentication(this NancyModule module) {
            injectAuthenticationHook(module, _adminClaim);
        }

        public static void injectAuthenticationHook(NancyModule module, Claim requiredClaim) {
            module.Before.AddItemToEndOfPipeline(ctx => {
                if (ctx.CurrentUser == null) {
                    return HttpStatusCode.Unauthorized;
                }

                if (ctx.CurrentUser.ensureClaim(requiredClaim)) {
                    return null;
                }

                return HttpStatusCode.Unauthorized;
            });
        }

        /// <summary>
        /// Ensures that a ClaimsPrincipal posesses a claim by checking the Type and Value fields
        /// </summary>
        /// <param name="principal"></param>
        /// <param name="claim"></param>
        /// <returns></returns>
        public static bool ensureClaim(this ClaimsPrincipal principal, Claim claim) {
            return principal.HasClaim(claim.Type, claim.Value);
        }
    }
}