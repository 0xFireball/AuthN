namespace AuthN.Models.Requests.User {
    public class UserKeyResetRequest {
        public string username { get; set; }

        public string apiKey { get; set; }
    }
}