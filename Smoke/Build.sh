CMD="";
if command -v csc &> /dev/null; then
    echo "'csc' command installed"
    CMD="csc"
elif command -v msc &> /dev/null; then
    echo "'msc' command installed"
    CMD="msc"
else
    echo "'csc' or 'msc' command needed for compilation. Have you installed 'mono'?"
    exit 1
fi

SCRIPT_DIR=$( cd -- "$( dirname -- "${BASH_SOURCE[0]}" )" &> /dev/null && pwd )
CURRENT_DIR=$(pwd)

cd $SCRIPT_DIR && {
    $CMD /unsafe -target:library \
        -r:"../lib/ScriptHookRDRNetAPI.dll" \
        -out:"bin/Smoke.net.dll" \
        "Smoke.cs" \
        "../utils/AnimationManager/AnimationManager.cs" \
        "../utils/FileLogger/FileLogger.cs" \
        "../utils/ItemManager/ItemManager.cs" \
        "../utils/PropManager/PropManager.cs"
} || {
    echo "There was an issue compiling the script"
    cd $CURRENT_DIR
    exit 1
}

echo "Script compiled to bin/Smoke.net.dll"
cd $CURRENT_DIR