namespace AuthN.Services.Auth {
    public class PasswordCryptoConfiguration {
        public int iterations { get; set; }

        public int length { get; set; }

        public int saltLength { get; set; }

        public static PasswordCryptoConfiguration createDefault() {
            return new PasswordCryptoConfiguration {
                iterations = DEFAULT_ITERATION_COUNT,
                length = DEFAULT_LENGTH,
                saltLength = DEFAULT_SALT_LENGTH
            };
        }

        public const int DEFAULT_ITERATION_COUNT = 10000;
        public const int DEFAULT_LENGTH = 128;
        public const int DEFAULT_SALT_LENGTH = 64;
    }
}