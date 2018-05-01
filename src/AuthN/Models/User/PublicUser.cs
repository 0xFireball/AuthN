using Newtonsoft.Json;

namespace AuthN.Models.User {
    /// <summary>
    /// Public user data model
    /// </summary>
    public class PublicUser : DatabaseObject {
        [JsonProperty("username")]
        public string username { get; set; }

        public PublicUser(RegisteredUser user) {
            username = user.username;
        }
    }
}