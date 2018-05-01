using LiteDB;
using Newtonsoft.Json;

namespace AuthN.Models {
    public class DatabaseObject {
        [JsonIgnore]
        [BsonId]
        public ObjectId databaseId { get; set; }
    }
}