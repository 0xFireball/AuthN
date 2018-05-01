namespace AuthN.Models.Requests.User {
    public class UserPasswordChangeRequest {
        public string username { get; set; }

        public string oldPassword { get; set; }

        public string newPassword { get; set; }
    }
}