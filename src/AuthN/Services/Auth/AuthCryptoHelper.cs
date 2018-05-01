using System.Security.Cryptography;
using System.Text;

namespace AuthN.Services.Auth {
    public class AuthCryptoHelper {
        private PasswordCryptoConfiguration configuration { get; }

        public AuthCryptoHelper(PasswordCryptoConfiguration conf) {
            configuration = conf;
        }

        public byte[] generateSalt() {
            var len = configuration.saltLength;
            var bytes = new byte[len];
            using (var rng = RandomNumberGenerator.Create()) {
                rng.GetBytes(bytes);
            }

            return bytes;
        }

        private byte[] calculatePasswordHash(byte[] password, byte[] salt) {
            var iter = configuration.iterations;
            var len = configuration.length;
            using (var deriveBytes = new Rfc2898DeriveBytes(password, salt, iter)) {
                return deriveBytes.GetBytes(len);
            }
        }

        public byte[] calculateUserPasswordHash(string password, byte[] salt) {
            var passwordBytes = Encoding.UTF8.GetBytes(password);
            return calculatePasswordHash(passwordBytes, salt);
        }

        public const int DEFAULT_API_KEY_LENGTH = 42;
    }
}