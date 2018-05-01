using Nancy;
using Nancy.ModelBinding;
using AuthN.Configuration;
using AuthN.Models.Requests.User;
using AuthN.Models.User;
using AuthN.Services.Auth;
using AuthN.Utilities;

namespace AuthN.Modules.User {
    public class PublicUserModule : SBaseModule {
        private UserManagerService _userManager;

        public PublicUserModule(ISContext serverContext) : base("/user", serverContext) {
            Before += ctx => {
                _userManager = new UserManagerService(this.serverContext);
                return null;
            };

            Get("/u/{username}", async args => {
                var user = await _userManager.findUserByUsernameAsync((string) args.username);
                if (user == null) return HttpStatusCode.NotFound;
                var publicProfile = new PublicUser(user);
                return Response.asJsonNet(publicProfile);
            });

            Get("/{id}", async args => {
                var user = await _userManager.findUserByIdentifierAsync((string) args.id);
                if (user == null) return HttpStatusCode.NotFound;
                var publicProfile = new PublicUser(user);
                return Response.asJsonNet(publicProfile);
            });
        }
    }
}