using System.Linq;
using System.Security.Claims;
using AuthN.Nfx.WebTokens;
using Nancy.Bootstrapper;

namespace AuthN.Nfx {
    public static class AuthenticationHook {
        public static void install(IPipelines pipelines, ITokenCryptoAlgorithm algorithm) {
            pipelines.BeforeRequest.AddItemToEndOfPipeline(ctx => {
                var authToken = ctx.Request.Headers.Authorization;
                if (string.IsNullOrEmpty(authToken)) return null;

                try {
                    var claims = new WebTokenBuilder()
                        .withAlgorithm(algorithm)
                        .mustVerify()
                        .decode(authToken);

                    ctx.CurrentUser =
                        new ClaimsPrincipal(
                            new ClaimsIdentity(claims.Select(x => new Claim(x.Key, x.Value.ToString())))
                        );

                    return null;
                } catch {
                    return null;
                }
            });
        }
    }
}