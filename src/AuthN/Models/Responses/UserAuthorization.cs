using AuthN.Models.User;

namespace AuthN.Models.Responses {
    public class UserAuthorization {
        public UserAuthorization(UserIdentity user, string authToken) {
            username = user.username;
            email = user.email;
            identifier = user.identifier;
            token = authToken;
        }

        public string username;
        public string email;
        public string identifier;
        public string token;
    }
}