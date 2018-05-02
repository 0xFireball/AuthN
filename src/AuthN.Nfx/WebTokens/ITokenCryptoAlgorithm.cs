namespace AuthN.Nfx.WebTokens {
    public interface ITokenCryptoAlgorithm {
        byte[] sign(byte[] data);
        bool verify(byte[] data, byte[] sig);
    }
}