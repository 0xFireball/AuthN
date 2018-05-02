using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace AuthN.Models.User {
    public class UserMetrics {
        public int apiRequests() => events.Count;
        public MetricsEvent lastRequest() => events.Last();

        [JsonProperty("events")]
        public List<MetricsEvent> events { get; set; } = new List<MetricsEvent>();
    }

    public struct MetricsEvent {
        public MetricsEventType type;
        public long time;

        public MetricsEvent(MetricsEventType type) {
            this.type = type;
            time = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        }
    }

    public enum MetricsEventType {
        Unspecified,
        Auth, // Login events
        UpdateAuth, // Update auth info, such as changing password
        UserApi,
    }
}