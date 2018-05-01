using Newtonsoft.Json;

namespace AuthN.Configuration {
    public class SDatabaseConfiguration {
        [JsonProperty("fileName")]
        public string fileName { get; } = "authn.lidb";
    }
}