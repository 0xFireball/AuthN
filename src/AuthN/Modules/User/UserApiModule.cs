using System.Linq;
using AuthN.Configuration;
using AuthN.Models.User;
using AuthN.Services.Auth;
using AuthN.Services.Auth.Security;
using AuthN.Services.Metrics;

namespace AuthN.Modules.User {
    /// <summary>
    /// Defines a module that is part of the **authenticated** user API.
    /// </summary>
    public abstract class UserApiModule : SBaseModule {
        public UserManagerService userManager { get; private set; }

        public UserMetricsService userMetrics { get; private set; }

        public RegisteredUser currentUser { get; private set; }

        internal UserApiModule(string path, ISContext serverContext) : base(path, serverContext) {
            // require claims from stateless auther, defined in bootstrapper
            this.requiresUserAuthentication();

            // add a pre-request hook to load the user manager
            Before += ctx => {
                var userIdentifier = Context.CurrentUser.Claims
                    .FirstOrDefault(x => x.Type == ApiAuthenticator.USER_IDENTIFIER_CLAIM_KEY)
                    ?.Value;

                userManager = new UserManagerService(this.serverContext);
                userMetrics = new UserMetricsService(this.serverContext, userIdentifier);
                currentUser = userManager.findUserByIdentifierAsync(userIdentifier).Result;
                return null;
            };
        }
    }
}