namespace AuthN.Services.Auth.Crypto {
    public interface ITokenCryptoAlgorithm {
        byte[] sign(byte[] data);
        bool verify(byte[] data, byte[] sig);
    }
}