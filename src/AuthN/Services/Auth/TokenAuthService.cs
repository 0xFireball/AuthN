using System;
using AuthN.Configuration;
using AuthN.Models.User;
using AuthN.Nfx.WebTokens;

namespace AuthN.Services.Auth {
    public class TokenAuthService : DependencyObject {
        public const string CLAIM_USERNAME = "username";
        public const string CLAIM_IDENTIFIER = "identifier";
        public const string CLAIM_GROUPS = "groups";
        public const string CLAIM_ADMIN = "admin";

        public TokenAuthService(ISContext context) : base(context) { }

        public string createToken(UserIdentity user) {
            var tokenBuilder = new WebTokenBuilder()
                .withAlgorithm(new RS384Algorithm(serverContext.configuration.crypto));
            // add user claims
            tokenBuilder
                .addClaim(CLAIM_USERNAME, user.username)
                .addClaim(CLAIM_IDENTIFIER, user.identifier)
                .addClaim(CLAIM_GROUPS, user.packGroups())
                .expire(DateTime.Now.Add(serverContext.configuration.tokenValidity));
            // check special users
            if (serverContext.configuration.admins.Contains(user.identifier)) {
                tokenBuilder.addClaim(CLAIM_ADMIN, true);
            }

            return tokenBuilder.build();
        }
    }
}