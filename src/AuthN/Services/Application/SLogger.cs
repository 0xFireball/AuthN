using System;

namespace AuthN.Services.Application {
    public class SLogger {
        public SLogger(LogLevel verbosity) {
            this.verbosity = verbosity;
        }

        public void writeLine(string log, LogLevel level) {
            if (level >= verbosity) {
                Console.WriteLine($"[{DateTime.Now}] [{level.ToString()}] {log}");
            }
        }

        public LogLevel verbosity = LogLevel.Warning;

        public enum LogLevel {
            Trace,
            Information,
            Warning,
            Error,
            Critical
        }
    }
}