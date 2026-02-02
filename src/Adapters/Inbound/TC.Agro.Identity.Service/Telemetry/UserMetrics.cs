using System.Diagnostics.Metrics;

namespace TC.Agro.Identity.Service.Telemetry
{
    /// <summary>
    /// User-centric metrics for Identity service.
    /// Tracks authentication events, user actions, and session metrics.
    /// </summary>
    public class UserMetrics
    {
        // Counters for user authentication events
        private readonly Counter<long> _userLogins;
        private readonly Counter<long> _userLogouts;
        private readonly Counter<long> _userCreations;
        private readonly Counter<long> _userActions;
        private readonly Counter<long> _authenticationFailures;

        // Histogram for session duration
        private readonly Histogram<double> _sessionDuration;

        // Up-down counter for active users
        private readonly UpDownCounter<long> _activeUsers;

        public UserMetrics()
        {
            var meter = new Meter(TelemetryConstants.IdentityMeterName, TelemetryConstants.Version);

            _userLogins = meter.CreateCounter<long>(
                "identity.user.logins_total",
                description: "Total number of user login events");

            _userLogouts = meter.CreateCounter<long>(
                "identity.user.logouts_total",
                description: "Total number of user logout events");

            _userCreations = meter.CreateCounter<long>(
                "identity.user.creations_total",
                description: "Total number of user creations");

            _userActions = meter.CreateCounter<long>(
                "identity.user.actions_total",
                description: "Total number of user actions (generic)");

            _authenticationFailures = meter.CreateCounter<long>(
                "identity.authentication.failures_total",
                description: "Total number of authentication failures");

            _sessionDuration = meter.CreateHistogram<double>(
                "identity.user.session_duration_seconds",
                unit: "s",
                description: "Duration of user sessions in seconds");

            _activeUsers = meter.CreateUpDownCounter<long>(
                "identity.users.active",
                description: "Number of currently active users");
        }

        /// <summary>
        /// Records a user login event
        /// </summary>
        public void RecordUserLogin(string userId)
        {
            _userLogins.Add(1,
                new KeyValuePair<string, object?>("user.id", userId));
            _activeUsers.Add(1,
                new KeyValuePair<string, object?>("user.id", userId));
        }

        /// <summary>
        /// Records a user logout event
        /// </summary>
        public void RecordUserLogout(string userId, double sessionDurationSeconds = 0)
        {
            _userLogouts.Add(1,
                new KeyValuePair<string, object?>("user.id", userId));
            _activeUsers.Add(-1,
                new KeyValuePair<string, object?>("user.id", userId));

            if (sessionDurationSeconds > 0)
            {
                _sessionDuration.Record(sessionDurationSeconds,
                    new KeyValuePair<string, object?>("user.id", userId));
            }
        }

        /// <summary>
        /// Records a user creation/registration event
        /// </summary>
        public void RecordUserCreation(string userId, string method = "email")
        {
            _userCreations.Add(1,
                new KeyValuePair<string, object?>("user.id", userId),
                new KeyValuePair<string, object?>("method", method));
        }

        /// <summary>
        /// Records a generic user action
        /// </summary>
        public void RecordUserAction(string actionType, string userId, string? details = null)
        {
            var tags = new List<KeyValuePair<string, object?>>
            {
                new("action.type", actionType),
                new("user.id", userId)
            };

            if (!string.IsNullOrWhiteSpace(details))
            {
                tags.Add(new("action.details", details));
            }

            _userActions.Add(1, tags.ToArray());
        }

        /// <summary>
        /// Records an authentication failure
        /// </summary>
        public void RecordAuthenticationFailure(string reason = "invalid_credentials")
        {
            _authenticationFailures.Add(1,
                new KeyValuePair<string, object?>("failure.reason", reason));
        }
    }
}
