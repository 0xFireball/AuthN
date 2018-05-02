using System;
using System.Collections.Generic;
using AuthN.Configuration;
using JWT;
using JWT.Builder;
using Nancy;

namespace AuthN.Modules.Extensions {
    public static class ModuleSecurityExtensions {
        public const string CTX_CLAIMS = "claims";

        public static IDictionary<string, object> getClaims(this NancyContext ctx) {
            return (IDictionary<string, object>) ctx.Items[CTX_CLAIMS];
        }
        
        public static IDictionary<string, object> assertClaims(this NancyModule module, ISContext context, params string[] requiredClaims) {
            var claims = default(IDictionary<string, object>);
            module.Before.AddItemToEndOfPipeline(ctx => {
                try {
                    var authToken = ctx.Request.Headers.Authorization;
                    if (string.IsNullOrEmpty(authToken)) return HttpStatusCode.Unauthorized;

                    claims = new JwtBuilder()
                        .WithSecret(context.configuration.jwtSecret)
                        .MustVerifySignature()
                        .Decode<IDictionary<string, object>>(authToken);

                    foreach (var requiredClaim in requiredClaims) {
                        if (!claims.ContainsKey(requiredClaim)) {
                            return HttpStatusCode.Unauthorized;
                        }
                    }
                    
                    ctx.Items["claims"] = claims;

                    return null; // continue, granted.
                } catch (TokenExpiredException) {
                    return HttpStatusCode.Unauthorized;
                } catch (SignatureVerificationException) {
                    return HttpStatusCode.Unauthorized;
                } catch (Exception) {
                    return HttpStatusCode.Unauthorized;
                }
            });
            return claims;
        }
    }
}