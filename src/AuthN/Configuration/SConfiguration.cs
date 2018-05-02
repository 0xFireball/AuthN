using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using AuthN.Services.Application;
using AuthN.Utilities;
using Newtonsoft.Json;

namespace AuthN.Configuration {
    public class SConfiguration {
        [JsonProperty("databaseConfig")]
        public SDatabaseConfiguration databaseConfiguration { get; set; } = new SDatabaseConfiguration();

        /// <summary>
        /// Whether to enable ASP.NET Core verbose logging
        /// </summary>
        [JsonProperty("aspnetVerboseLogging")]
        public bool aspnetVerboseLogging { get; set; } = true;

        /// <summary>
        /// If an invite key is required for registration
        /// </summary>
        [JsonProperty("requireInvite")]
        public bool inviteRequired { get; set; }

        /// <summary>
        /// Special API keys that grant privileged admin access (keep these secret!)
        /// </summary>
        [JsonProperty("adminKeys")]
        public string[] adminKeys { get; set; } = new string[0];
        
        /// <summary>
        /// The identifier for this authentication server
        /// </summary>
        [JsonProperty("authServerId")]
        public string authServerId { get; set; } = StringUtils.secureRandomString(12);

        /// <summary>
        /// Web token private key
        /// </summary>
        [JsonProperty("privateKey")]
        public string privateKey { get; set; } = null;

        /// <summary>
        /// List of origins to allow CORS requests from. Can possibly be used to enable API access from another domain hosting a custom client.
        /// </summary>
        [JsonProperty("corsOrigins")]
        public List<string> corsOrigins { get; set; } = new List<string>();

        /// <summary>
        /// List of user identifiers to grant admin permissions
        /// </summary>
        [JsonProperty("admins")]
        public List<string> admins { get; set; } = new List<string>();

        /// <summary>
        /// Maximum number of registered users. Set to -1 for unlimited.
        /// </summary>
        [JsonProperty("maxUsers")]
        public int maxUsers { get; set; } = -1;

        /// <summary>
        /// Timespan (expiry) for authentication tokens.
        /// </summary>
        [JsonProperty("tokenValidity")]
        public TimeSpan tokenValidity { get; set; } = TimeSpan.FromDays(1);

        /// <summary>
        /// The verbosity of the application logger.
        /// </summary>
        [JsonProperty("logLevel")]
        public SLogger.LogLevel logLevel { get; set; } = SLogger.LogLevel.Information;

        /// <summary>
        /// Database persistence interval in milliseconds.
        /// </summary>
        [JsonProperty("persistenceInterval")]
        public int persistenceInterval { get; set; } = 1000 * 60;

        /// <summary>
        /// RSA Crypto parameters, imported from privateKey on startup
        /// </summary>
        [JsonIgnore]
        public RSA crypto { get; set; }
    }
}