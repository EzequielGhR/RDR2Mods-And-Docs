#ifndef FILE_LOGGER_H
    #define FILE_LOGGER_H

    #include <string.h>
    #include <stdarg.h>
    #include <unordered_map>

    static std::unordered_map<const char*, bool> _initialized = {};

    bool _createFile(const char* fpath);
    bool _writeFile(const char* fpath, const char* lvl, const char* loggerName, const char* serviceName, const char* format, va_list args);

    class FileLogger {
        private:
            const char* logPath;
            const char* loggerName;
            const char* serviceName;

        public:
            FileLogger(const char* lpath, const char* lName, const char* sName = nullptr) {
                logPath = lpath;
                loggerName = lName;
                serviceName = sName != nullptr && strlen(sName) > 0 ? sName : "root";
                
                bool initialized = _initialized.find(logPath) != _initialized.end() && _initialized[logPath];
                if (!initialized) {
                    if (_createFile(logPath)) {
                        _initialized.insert_or_assign(logPath, true);
                    }
                }
            }

            void Log(const char* format, ...) {
                va_list args;
                va_start(args, format);
                _writeFile(logPath, "INFO", loggerName, serviceName, format, args);
                va_end(args);
            }

            void Warn(const char* format, ...) {
                va_list args;
                va_start(args, format);
                _writeFile(logPath, "WARNING", loggerName, serviceName, format, args);
                va_end(args);
            }

            void Error(const char* format, ...) {
                va_list args;
                va_start(args, format);
                _writeFile(logPath, "ERROR", loggerName, serviceName, format, args);
                va_end(args);
            }
    };
    
#endif