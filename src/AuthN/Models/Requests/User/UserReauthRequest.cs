namespace AuthN.Models.Requests.User {
    public class UserReauthRequest {
        public string username { get; set; }

        public string apiKey { get; set; }
    }
}