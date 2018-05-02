using AuthN.Configuration;
using AuthN.Models.Requests.Admin;
using AuthN.Modules.Extensions;
using AuthN.Services.Auth;
using AuthN.Utilities;
using Nancy;
using Nancy.ModelBinding;

namespace AuthN.Modules.Admin {
    public class AdministrationModule : SBaseModule {
        private UserManagerService userManager;

        public AdministrationModule(ISContext serverContext) : base("/admin", serverContext) {
            Before += ctx => {
                userManager = new UserManagerService(serverContext);
                return null;
            };
            this.assertClaims(serverContext, TokenAuthService.CLAIM_ADMIN);
            
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
        }
    }
}