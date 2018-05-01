using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using AuthN.Configuration;
using AuthN.Services.Metrics;

namespace AuthN.Services.Auth {
    public class ApiAuthenticator : DependencyObject {
        public const string AUTH_TYPE_CLAIM_KEY = "auth_type";

        public const string USER_IDENTIFIER_CLAIM_KEY = "user_id";

        public const string API_KEY_CLAIM_KEY = "api_key";

        public ApiAuthenticator(ISContext context) : base(context) { }

        public ClaimsPrincipal resolveIdentity(string apikey) {
            // check admin keys
            var adminKey = serverContext.configuration.adminKeys.FirstOrDefault(x => x == apikey);
            if (adminKey != null) {
                var adminAuthClaims = new List<Claim> {
                    new Claim(AUTH_TYPE_CLAIM_KEY, ApiAccessScope.Admin.ToString()),
                    new Claim(API_KEY_CLAIM_KEY, adminKey)
                };
                var adminAuthIdentity = new ClaimsIdentity(adminAuthClaims);
                var adminIdentity = new ClaimsPrincipal(adminAuthIdentity);
                return adminIdentity;
            }

            // get user identity
            var userManager = new UserManagerService(serverContext);
            var user = userManager.findUserByApiKeyAsync(apikey).Result;
            if (user == null) return null;

            var metricsService = new UserMetricsService(serverContext, user.identifier);
            var metricsObject = metricsService.get();
            metricsObject.apiRequests++;
            metricsObject.lastRequest = (ulong) DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            var userAuthClaims = new List<Claim> {
                new Claim(AUTH_TYPE_CLAIM_KEY, ApiAccessScope.User.ToString()),
                new Claim(USER_IDENTIFIER_CLAIM_KEY, user.identifier),
                new Claim(API_KEY_CLAIM_KEY, apikey)
            };
            var userAuthIdentity = new ClaimsIdentity(userAuthClaims);
            var userIdentity = new ClaimsPrincipal(userAuthIdentity);
            // TODO: Maybe more claims?
            return userIdentity;
        }
    }
}