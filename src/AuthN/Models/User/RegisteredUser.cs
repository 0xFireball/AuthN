using Newtonsoft.Json;

namespace AuthN.Models.User {
    public class RegisteredUser : DatabaseObject {
        [JsonProperty("username")]
        public string username { get; set; }

        [JsonProperty("email")]
        public string email { get; set; }

        [JsonProperty("apikey")]
        public string apiKey { get; set; }

        [JsonIgnore]
        public ItemCrypto crypto { get; set; }

        [JsonIgnore]
        public string identifier { get; set; }

        [JsonIgnore]
        public bool enabled { get; set; }
    }
}