using Newtonsoft.Json;

namespace AuthN.Models.User {
    public class UserIdentity : DatabaseObject {
        [JsonProperty("username")]
        public string username { get; set; }

        [JsonProperty("email")]
        public string email { get; set; }
        
        [JsonIgnore]
        public ItemCrypto crypto { get; set; }

        [JsonIgnore]
        public string identifier { get; set; }

        [JsonIgnore]
        public bool enabled { get; set; }
    }
}