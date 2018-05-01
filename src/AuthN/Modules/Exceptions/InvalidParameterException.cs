using System;

namespace AuthN.Modules.Exceptions {
    public class InvalidParameterException : Exception {
        public InvalidParameterException(string message) : base(message) { }
    }
}