namespace AuthN.Models.Requests.User {
    public class UserRegistrationRequest {
        public string username { get; set; }

        public string email { get; set; }

        public string password { get; set; }

        public string inviteKey { get; set; }
    }
}