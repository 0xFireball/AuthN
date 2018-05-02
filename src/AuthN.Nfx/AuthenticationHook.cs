using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using AuthN.Nfx.WebTokens;
using JWT;
using Nancy.Bootstrapper;

namespace AuthN.Nfx {
    public static class AuthenticationHook {
        public static void install(IPipelines pipelines, IEnumerable<ITokenCryptoAlgorithm> algorithms) {
            pipelines.BeforeRequest.AddItemToEndOfPipeline(ctx => {
                var authToken = ctx.Request.Headers.Authorization;
                if (string.IsNullOrEmpty(authToken)) return null;
                foreach (var algorithm in algorithms) {
                    try {
                        var claims = new WebTokenBuilder()
                            .withAlgorithm(algorithm)
                            .mustVerify()
                            .decode(authToken);

                        ctx.CurrentUser =
                            new ClaimsPrincipal(
                                new ClaimsIdentity(claims.Select(x => new Claim(x.Key, x.Value.ToString())))
                            );
                        ctx.Items["claims"] = claims;

                        return null;
                    } catch (TokenExpiredException) {
                        return null; // no auth
                    } catch (SignatureVerificationException) {
                        continue; // try the next algo
                    }
                }

                return null; // failure
            });
        }
    }
}