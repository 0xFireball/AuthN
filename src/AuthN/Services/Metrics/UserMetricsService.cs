using AuthN.Configuration;
using AuthN.Models.User;

namespace AuthN.Services.Metrics {
    public class UserMetricsService : DependencyObject {
        private readonly string _userIdentifier;

        public UserMetricsService(ISContext context, string userIdentifier) : base(context) {
            _userIdentifier = userIdentifier;
        }

        public UserMetrics get() {
            return serverContext.appState.userMetrics[_userIdentifier];
        }

        public void logEvent(MetricsEventType eventType = MetricsEventType.Unspecified) {
            var metrics = get();
            metrics.events.Add(new MetricsEvent(eventType));
        }
    }
}