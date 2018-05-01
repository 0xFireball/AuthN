using Nancy;
using Nancy.ModelBinding;
using AuthN.Configuration;
using AuthN.Models.Requests.User;
using AuthN.Models.User;
using AuthN.Utilities;

namespace AuthN.Modules.User {
    public class UserMetadataModule : UserApiModule {
        public UserMetadataModule(ISContext serverContext) : base("/user", serverContext) {
            Get("/", _ => Response.asJsonNet(currentUser));

            Put("/", async _ => {
                var req = this.Bind<UserModificationRequest>();
                req.apply(currentUser);
                await userManager.updateUserInDatabaseAsync(currentUser);
                return Response.asJsonNet(currentUser);
            });
        }
    }
}