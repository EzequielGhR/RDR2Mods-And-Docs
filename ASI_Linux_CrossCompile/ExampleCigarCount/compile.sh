#!/bin/bash

if command -v clang-cl &> /dev/null; then
    echo "clang-cl command present"
else
    echo "clang-cl command not present, have you installed clang?"
    exit 1;
fi

if command -v lld-link &> /dev/null; then
    echo "lld-link command present"
else
    echo "lld-link command not present, have you installed lld?"
    exit 1;
fi

if [ -z $XWIN_CACHE ]; then
    echo "XWIN_CACHE env var is not defined"
    exit 1
fi

SCRIPT_DIR=$( cd -- "$( dirname -- "${BASH_SOURCE[0]}" )" &> /dev/null && pwd )
CURRENT_DIR=$(pwd)

cd $SCRIPT_DIR

out_path="bin/cigCount.asi"

mkdir -p bin && clang-cl --target=x86_64-pc-windows-msvc \
    /vctoolsdir "${XWIN_CACHE}/splat/crt" \
    /winsdkdir "${XWIN_CACHE}/splat/sdk" \
    -fuse-ld=lld \
    /LD \
    /EHsc \
    /O2 \
    main.cpp ../SDK/lib/ScriptHookRDR2.lib \
    /link /OUT:$out_path


if [ $? -eq 0 ]; then
    echo "Compiled into ${out_path}"
else
    echo "There was an issue compiling the plugin"
fi

cd $CURRENT_DIR