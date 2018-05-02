using AuthN.Infrastructure.Concurrency;
using AuthN.Services.Application;
using LiteDB;

namespace AuthN.Configuration {
    public class SContext : ISContext {
        // Configuration parameters
        public SConfiguration configuration { get; }

        // Database access
        public LiteDatabase database { get; private set; }

        // Persistent State
        public SAppState appState { get; set; }

        // Service table
        public UserServiceTable serviceTable { get; }
        public SLogger log { get; }

        public const string version = "0.0.1-dev";

        public SContext(SConfiguration config) {
            configuration = config;
            serviceTable = new UserServiceTable(this);
            log = new SLogger(config.logLevel);
        }

        public void connectDatabase() {
            // Create database
            database = new LiteDatabase(configuration.databaseConfiguration.fileName);
            // load dependent services
            loadDatabaseDependentServices();
        }

        private void loadDatabaseDependentServices() {
            // ...
        }
    }
}