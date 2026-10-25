#!/bin/bash
csc -target:library -out:"bin/AnimationManager.net.dll" -r:"../../lib/ScriptHookRDRNetAPI.dll" AnimationManager.cs ../FileLogger/FileLogger.cs