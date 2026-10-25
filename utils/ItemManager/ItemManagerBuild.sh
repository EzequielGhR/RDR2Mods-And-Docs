#!/bin/bash
csc -target:library -out:"bin/ItemManager.net.dll" -r:"../../lib/ScriptHookRDRNetAPI.dll" -r:"../FileLogger/bin/FileLogger.net.dll" ItemManager.cs