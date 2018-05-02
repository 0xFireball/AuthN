using AuthN.Configuration;
using AuthN.Models.User;
using JWT.Algorithms;
using JWT.Builder;

namespace AuthN.Services.Auth {
    public class TokenAuthService : DependencyObject {
        public TokenAuthService(ISContext context) : base(context) { }

        public string createToken(UserIdentity user) {
            var tokenBuilder = new JwtBuilder()
                .WithAlgorithm(new HMACSHA384Algorithm())
                .WithSecret(serverContext.configuration.jwtSecret);
            // add user claims
            tokenBuilder
                .AddClaim("username", user.username);
            return tokenBuilder.Build();
        }
    }
}