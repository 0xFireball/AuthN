using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using LiteDB;

namespace AuthN.Configuration {
    public static class SConfigurator {
        internal static SContext createContext(SConfiguration config) {
            var context = new SContext(config);
            return context;
        }

        private const string state_storage_key = "state";

        private static bool serializationMappersRegistered { get; set; }

        public static void loadState(SContext serverContext, string stateStorageFile) {
            if (!serializationMappersRegistered) {
                // Register any needed mappers here
                serializationMappersRegistered = true;
            }

            // Load the Server State into the context. This object also includes the OsmiumMine Core state
            var database = new LiteDatabase(stateStorageFile);
            var stateStorage = database.GetCollection<SAppState>(state_storage_key);
            var savedState = stateStorage.FindAll().FirstOrDefault();
            if (savedState == null) {
                // Create and save new state
                savedState = new SAppState();
                stateStorage.Upsert(savedState);
            }

            // Update context
            savedState.persistenceMedium = stateStorage;
            savedState.persist = forcePersist => {
                // If needed...
                if (forcePersist || savedState.persistNeeded) {
                    savedState.persistAvailable = false;
                    // Update in database
                    stateStorage.Upsert(savedState);
                    // And unset needed flag
                    savedState.persistNeeded = false;
                    savedState.persistAvailable = true;
                }
            };
            // Save the state
            savedState.persist(true);
            // Update references
            serverContext.appState = savedState;
            var timedPersistTask = startTimedPersistAsync(serverContext, savedState);
        }

        private static async Task startTimedPersistAsync(SContext serverContext, SAppState state) {
            while (true) {
                if (state.persistAvailable) {
                    await Task.Delay(serverContext.configuration.persistenceInterval);
                    state.persist(false);
                }
            }
        }
    }
}