#!/bin/bash
csc -target:library -out:"bin/PropManager.net.dll" -r:"../../lib/ScriptHookRDRNetAPI.dll" PropManager.cs ../FileLogger/FileLogger.cs