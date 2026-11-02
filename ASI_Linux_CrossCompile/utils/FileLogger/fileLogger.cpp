#include <stdio.h>
#include "fileLogger.h"

bool _createFile(const char* fpath) {
    FILE* f = fopen(fpath, "w");
    if (f != NULL) return false;

    fclose(f);
    return true;
}

bool _writeFile(const char* fpath, const char* lvl, const char* loggerName, const char* serviceName, const char* format, va_list args) {
    FILE* f = fopen(fpath, "a");
    if (f == NULL) return false;

    char buffer[1024];

    _lock_file(f);
    vsnprintf((char*)buffer, 1023, format, args);
    fprintf(f, "[%s][%s:%s] %s\n", lvl, loggerName, serviceName, buffer);
    _unlock_file(f);

    fclose(f);

    return true;
}