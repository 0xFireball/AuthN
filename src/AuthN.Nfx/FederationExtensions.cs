using System.Collections.Generic;

namespace AuthN.Nfx {
    public static class FederationExtensions {
        public static string getFederatedUsername(IDictionary<string, object> claims) {
            return claims["username"] + "@" + claims["server"];
        }
    }
}