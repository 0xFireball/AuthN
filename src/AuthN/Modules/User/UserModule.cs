using System.Collections.Generic;
using System.Linq;
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
        private UserIdentity user;

        public UserModule(ISContext serverContext) : base("/user", serverContext) {
            this.assertClaims(serverContext, TokenAuthService.CLAIM_USERNAME);
            Before += ctx => {
                userManager = new UserManagerService(serverContext);
                // update metrics
                new UserMetricsService(serverContext,
                        ctx.CurrentUser.getClaim(TokenAuthService.CLAIM_IDENTIFIER))
                    .logEvent(MetricsEventType.UserApi);
                user = userManager.findUserByIdentifierAsync(ctx.CurrentUser.getClaim(TokenAuthService.CLAIM_IDENTIFIER))
                    .Result;
                return null;
            };

            Get("/", async _ => {
                return Response.asJsonNet(user);
            });

            Put("/", async _ => {
                var req = this.Bind<UserModificationRequest>();
                req.apply(user);
                await userManager.updateUserInDatabaseAsync(user);
                return HttpStatusCode.NoContent;
            });
        }
    }
}