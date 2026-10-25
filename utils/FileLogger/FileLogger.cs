using System;
using System.Collections.Concurrent;
using System.IO;
using System.Text;

namespace FileLogger {
    public class FileLogger {
        // Simple way to define log levels
        private const string INFO = "INFO";
        private const string WARNING = "WARNING";
        private const string ERROR = "ERROR";
        
        private readonly string filePath;
        private readonly string logName;
        private readonly string serviceName = null;
        private static readonly object _fileLock = new object();
        private static readonly ConcurrentDictionary<string, bool> _initialized = new ConcurrentDictionary<string, bool>();

        public FileLogger(string filePath, string logName, string serviceName = null) {
            this.filePath = filePath;
            this.logName = logName;
            this.serviceName = serviceName;

            if (_initialized.TryAdd(filePath, true)) {
                lock (_fileLock) {
                    using (new StreamWriter(filePath, append: false)) {}
                }
            }
        }

        public void Log(string format, params object[] objects) {
            LogByLevel(INFO, format, objects);
        }

        public void Warn(string format, params object[] objects) {
            LogByLevel(WARNING, format, objects);
        }

        public void Error(string format, params object[] objects) {
            LogByLevel(ERROR, format, objects);
        }

        private void LogByLevel(string level, string format, params object[] objects) {
            string msg = String.Format(format, objects);
            string logMsg = String.Format("[{0}][{1}:{2}] {3}", level, logName, serviceName ?? "root", msg);

            lock (_fileLock) {
                using (StreamWriter writer = new StreamWriter(filePath, append: true, encoding: Encoding.UTF8)) {
                    writer.WriteLine(logMsg);
                }
            }
        }
    }
}