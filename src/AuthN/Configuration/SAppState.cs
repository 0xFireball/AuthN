using System;
using System.Collections.Generic;
using AuthN.Models;
using AuthN.Models.User;
using LiteDB;

// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global

namespace AuthN.Configuration {
    /// <summary>
    /// Persisted state for the server
    /// </summary>
    public class SAppState : DatabaseObject {
        [BsonIgnore]
        public LiteCollection<SAppState> persistenceMedium { get; set; }

        /// <summary>
        /// Persist the data container to disk. Call QueuePersist() if requesting a persist.
        /// </summary>
        /// <returns></returns>
        [BsonIgnore]
        public Action<bool> persist { get; set; }

        [BsonIgnore]
        public bool persistNeeded { get; set; }

        [BsonIgnore]
        public bool persistAvailable { get; set; } = true;

        /// <summary>
        /// Call this to queue a persist.
        /// </summary>
        public void queuePersist() {
            persistNeeded = true;
        }

        public Dictionary<string, UserMetrics> userMetrics { get; set; } = new Dictionary<string, UserMetrics>();
    }
}