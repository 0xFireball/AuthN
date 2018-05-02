using System;
using System.Collections.Generic;
using System.Text;
using JWT;
using Newtonsoft.Json;

namespace AuthN.Services.Auth.Crypto {
    public class WebTokenBuilder {
        private ITokenCryptoAlgorithm algorithm;
        private Dictionary<string, object> claims = new Dictionary<string, object>();
        private bool _mustVerify;

        public WebTokenBuilder withAlgorithm(ITokenCryptoAlgorithm algorithm) {
            this.algorithm = algorithm;
            return this;
        }

        public WebTokenBuilder addClaim(string name, object value) {
            claims.Add(name, value);
            return this;
        }
        
        public WebTokenBuilder expire(DateTime expiry) {
            return addClaim("exp", expiry);
        }

        public string build() {
            var payload = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(claims));
            var signature = algorithm.sign(payload);
            var token = Convert.ToBase64String(payload) + "." + Convert.ToBase64String(signature);
            return token;
        }

        public WebTokenBuilder mustVerify() {
            _mustVerify = true;
            return this;
        }

        public IDictionary<string, object> decode(string token) {
            var tokenParts = token.Split('.');
            var payload = Convert.FromBase64String(tokenParts[0]);
            var signature = Convert.FromBase64String(tokenParts[1]);
            if (_mustVerify) {
                if (!algorithm.verify(payload, signature)) {
                    throw new SignatureVerificationException("signature did not match the data");
                }
            }

            var data = JsonConvert.DeserializeObject<IDictionary<string, object>>(Encoding.UTF8.GetString(payload));
            if (data.ContainsKey("exp")) {
                var expiration = (DateTime) data["exp"];
                if (expiration < DateTime.Now) {
                    throw new TokenExpiredException($"token expired at {expiration}");
                }
            }

            return data;
        }
    }
}