using AuthN.Configuration;
using AuthN.Models.Requests.Admin;
using AuthN.Modules.Extensions;
using AuthN.Services.Auth;
using AuthN.Services.Metrics;
using AuthN.Utilities;
using Nancy;
using Nancy.ModelBinding;

namespace AuthN.Modules.Admin {
    public class AdministrationModule : SBaseModule {
        private UserManagerService userManager;

        public AdministrationModule(ISContext serverContext) : base("/admin", serverContext) {
            this.assertClaims(serverContext, TokenAuthService.CLAIM_ADMIN);
            Before += ctx => {
                userManager = new UserManagerService(serverContext);
                return null;
            };

            Get("/user/{id}", async args => {
                var user = await userManager.findUserByIdentifierAsync((string) args.id);
                return Response.asJsonNet(user);
            });

            Put("/user/{id}", async args => {
                var user = await userManager.findUserByIdentifierAsync((string) args.id);
                var req = this.Bind<AdminUserModificationRequest>();
                req.apply(user);
                await userManager.updateUserInDatabaseAsync(user);
                return HttpStatusCode.NoContent;
            });

            Get("/metrics/{id}", async args => {
                var user = await userManager.findUserByIdentifierAsync((string) args.id);
                var metrics = new UserMetricsService(serverContext, user.identifier).get();
                return Response.asJsonNet(metrics);
            });
        }
    }
}