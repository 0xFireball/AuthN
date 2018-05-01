using AuthN.Services.Auth;

namespace AuthN.Models.User {
    public class ItemCrypto {
        public PasswordCryptoConfiguration conf { get; set; }

        public byte[] salt { get; set; }

        public byte[] key { get; set; }
    }
}