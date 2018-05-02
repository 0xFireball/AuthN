using AuthN.Nfx;
using Nancy;

namespace FederationDemo.Modules {
    public class FederationShowModule : NancyModule {
        public FederationShowModule() {
            this.assertClaims("username");

            Get("/me", _ => {
                var userId =
                    FederationExtensions.getFederatedUsername(Context.getContextClaims());
                return userId;
            });
        }
    }
}