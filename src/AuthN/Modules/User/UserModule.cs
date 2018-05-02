using System;
using System.Collections.Generic;
using AuthN.Configuration;
using AuthN.Models.Requests.User;
using AuthN.Models.User;
using AuthN.Modules.Extensions;
using AuthN.Services.Auth;
using AuthN.Services.Metrics;
using AuthN.Utilities;
using Nancy;
using Nancy.ModelBinding;

namespace AuthN.Modules.User {
    public class UserModule : SBaseModule {
        private UserManagerService userManager;
        private IDictionary<string, object> claims;

        public UserModule(ISContext serverContext) : base("/user", serverContext) {
            this.assertClaims(serverContext, TokenAuthService.CLAIM_USERNAME);
            Before += ctx => {
                userManager = new UserManagerService(serverContext);
                claims = ctx.getClaims();
                // update metrics
                new UserMetricsService(serverContext, (string) claims[TokenAuthService.CLAIM_IDENTIFIER])
                    .logEvent(MetricsEventType.UserApi);
                return null;
            };

            Get("/", async _ => {
                var user = await userManager.findUserByUsernameAsync((string) claims[TokenAuthService.CLAIM_USERNAME]);
                return Response.asJsonNet(user);
            });

            Put("/", async _ => {
                var user = await userManager.findUserByUsernameAsync((string) claims[TokenAuthService.CLAIM_USERNAME]);
                var req = this.Bind<UserModificationRequest>();
                req.apply(user);
                await userManager.updateUserInDatabaseAsync(user);
                return HttpStatusCode.NoContent;
            });
        }
    }
}