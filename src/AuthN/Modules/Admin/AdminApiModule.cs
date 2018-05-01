using System.Linq;
using AuthN.Configuration;
using AuthN.Services.Application;
using AuthN.Services.Auth;
using AuthN.Services.Auth.Security;

namespace AuthN.Modules.Admin {
    /// <summary>
    /// Defines a module that is part of the **authenticated** user API.
    /// </summary>
    public abstract class AdminApiModule : SBaseModule {
        public UserManagerService userManager { get; private set; }

        internal AdminApiModule(string path, ISContext serverContext) : base($"/admin{path}", serverContext) {
            // require claims from stateless auther, defined in bootstrapper
            this.requiresAdminAuthentication();

            // add a pre-request hook to load the user manager
            Before += ctx => {
                // retrieve the key used for admin auth for logging purposes
                var adminKey = Context.CurrentUser.Claims
                    .FirstOrDefault(x => x.Type == ApiAuthenticator.API_KEY_CLAIM_KEY)
                    ?.Value;
                serverContext.log.writeLine($"Admin authenticated using key {adminKey}",
                    SLogger.LogLevel.Information);

                userManager = new UserManagerService(this.serverContext);
                return null;
            };
        }
    }
}