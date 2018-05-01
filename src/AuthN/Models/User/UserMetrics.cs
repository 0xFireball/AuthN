using Newtonsoft.Json;

namespace AuthN.Models.User {
    public class UserMetrics {
        [JsonProperty("apiRequests")]
        public ulong apiRequests { get; set; }
        [JsonProperty("lastRequest")]
        public ulong lastRequest { get; set; }
        [JsonProperty("lastConnection")]
        public ulong lastConnection { get; set; }
    }
}