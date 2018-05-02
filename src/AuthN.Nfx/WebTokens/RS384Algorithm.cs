using System.Security.Cryptography;

namespace AuthN.Nfx.WebTokens {
    public class RS384Algorithm : ITokenCryptoAlgorithm {
        public RSA rsa { get; }

        public RS384Algorithm(RSA rsa) {
            this.rsa = rsa;
        }

        public byte[] sign(byte[] data) {
            return rsa.SignData(data, HashAlgorithmName.SHA384, RSASignaturePadding.Pkcs1);
        }

        public bool verify(byte[] data, byte[] sig) {
            return rsa.VerifyData(data, sig, HashAlgorithmName.SHA384, RSASignaturePadding.Pkcs1);
        }
    }
}