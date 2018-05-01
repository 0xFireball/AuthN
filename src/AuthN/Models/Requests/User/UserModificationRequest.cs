using AuthN.Models.User;

namespace AuthN.Models.Requests.User {
    public class UserModificationRequest {
        public string email { get; set; } = string.Empty;

        public void apply(RegisteredUser user) {
            user.email = email;
        }
    }
}