using System;
using AuthN.Configuration;
using AuthN.Models.User;
using JWT.Algorithms;
using JWT.Builder;

namespace AuthN.Services.Auth {
    public class TokenAuthService : DependencyObject {
        public const string CLAIM_USERNAME = "username";
        public const string CLAIM_IDENTIFIER = "identifier";
        public const string CLAIM_GROUPS = "groups";
        public const string CLAIM_ADMIN = "admin";
        
        public TokenAuthService(ISContext context) : base(context) { }

        public string createToken(UserIdentity user) {
            var tokenBuilder = new JwtBuilder()
                .WithAlgorithm(new HMACSHA384Algorithm())
                .WithSecret(serverContext.configuration.jwtSecret);
            // add user claims
            tokenBuilder
                .AddClaim(CLAIM_USERNAME, user.username)
                .AddClaim(CLAIM_IDENTIFIER, user.identifier)
                .AddClaim(CLAIM_GROUPS, user.packGroups())
                .ExpirationTime(DateTime.Now.Add(serverContext.configuration.tokenValidity));
            // check special users
            if (serverContext.configuration.admins.Contains(user.identifier)) {
                tokenBuilder.AddClaim(CLAIM_ADMIN, true);
            }
            return tokenBuilder.Build();
        }
    }
}