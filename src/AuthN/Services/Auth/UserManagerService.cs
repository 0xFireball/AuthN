using System;
using System.Collections;
using System.Security;
using System.Threading.Tasks;
using LiteDB;
using AuthN.Configuration;
using AuthN.Models.User;
using AuthN.Models.Requests.User;
using AuthN.Utilities;

namespace AuthN.Services.Auth {
    public class UserManagerService : DependencyObject {
        public const string REGISTERED_USERS_KEY = "r_users";
        private LiteCollection<UserIdentity> _userCollection;

        public UserManagerService(ISContext serverContext) : base(serverContext) {
            _userCollection = serverContext.database.GetCollection<UserIdentity>(REGISTERED_USERS_KEY);
        }

        public async Task<UserIdentity> registerUserAsync(UserRegistrationRequest regRequest) {
            if (await findUserByUsernameAsync(regRequest.username) != null) {
                throw new SecurityException("a user with the same username already exists");
            }
            // Calculate cryptographic info
            var cryptoConf = PasswordCryptoConfiguration.createDefault();
            var cryptoHelper = new AuthCryptoHelper(cryptoConf);
            var pwSalt = cryptoHelper.generateSalt();
            var encryptedPassword =
                cryptoHelper.calculateUserPasswordHash(regRequest.password, pwSalt);
            // Create user
            var user = new UserIdentity {
                identifier = Guid.NewGuid().ToString(),
                username = regRequest.username,
                email = regRequest.email,
                crypto = new ItemCrypto {
                    salt = pwSalt,
                    conf = cryptoConf,
                    key = encryptedPassword
                },
                enabled = true
            };

            // Add the user to the database
            _userCollection.Insert(user);

            // Index database
            _userCollection.EnsureIndex(x => x.identifier);
            _userCollection.EnsureIndex(x => x.username);

            serverContext.appState.userMetrics[user.identifier] = new UserMetrics();

            return user;
        }

        public async Task<UserIdentity> findUserByUsernameAsync(string username) {
            return await Task.Run(() => (_userCollection.FindOne(x => x.username == username)));
        }

        public async Task<UserIdentity> findUserByIdentifierAsync(string id) {
            return await Task.Run(() => (_userCollection.FindOne(x => x.identifier == id)));
        }

        /// <summary>
        /// Warning: Callers are expected to use their own locks before calling this method!
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public async Task<bool> updateUserInDatabaseAsync(UserIdentity user) {
            return await Task.Run(() => _userCollection.Update(user));
        }

        public async Task<bool> checkPasswordAsync(string password, UserIdentity user) {
            var ret = false;
            var lockEntry = serverContext.serviceTable.getOrCreate(user.username).userLock;
            await lockEntry.withConcurrentReadAsync(Task.Run(() => {
                //Calculate hash and compare
                var cryptoHelper = new AuthCryptoHelper(user.crypto.conf);
                var pwKey =
                    cryptoHelper.calculateUserPasswordHash(password, user.crypto.salt);
                ret = StructuralComparisons.StructuralEqualityComparer.Equals(pwKey, user.crypto.key);
            }));
            return ret;
        }

        public async Task changeUserPasswordAsync(UserIdentity user, string newPassword) {
            var lockEntry = serverContext.serviceTable.getOrCreate(user.username).userLock;
            await lockEntry.withExclusiveWriteAsync(Task.Run(async () => {
                // Recompute password crypto
                var cryptoConf = PasswordCryptoConfiguration.createDefault();
                var cryptoHelper = new AuthCryptoHelper(cryptoConf);
                var pwSalt = cryptoHelper.generateSalt();
                var encryptedPassword =
                    cryptoHelper.calculateUserPasswordHash(newPassword, pwSalt);
                user.crypto = new ItemCrypto {
                    salt = pwSalt,
                    conf = cryptoConf,
                    key = encryptedPassword
                };

                // Save changes
                await updateUserInDatabaseAsync(user);
            }));
        }

        public Task<bool> deleteUserAsync(string userId) {
            var result = _userCollection.Delete(x => x.identifier == userId) > 0;
            if (result) {
                serverContext.appState.userMetrics.Remove(userId);
            }
            return Task.FromResult(result);
        }

        public async Task setEnabledAsync(UserIdentity user, bool status) {
            var lockEntry = serverContext.serviceTable.getOrCreate(user.username).userLock;
            await lockEntry.obtainExclusiveWriteAsync();
            user.enabled = status;
            await updateUserInDatabaseAsync(user);
            lockEntry.releaseExclusiveWrite();
        }

        public int userIdentityCount => _userCollection.Count();
    }
}