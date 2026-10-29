# What is this?

Some sample code and knowledge to cross-compile your own .asi plugins from linux

# Requirements

- `ScriptHookRDR2-SDK` by Alexander Blade, he does not allow redistribution so I am not including it in the repo, you can get it here:
https://www.dev-c.com/rdr2/scripthookrdr2/
- `clang-cl` and `lld-link`, this depends on your distro. Arch based systems can install them via pacman.
- `xwin`, arch users can install via AUR like with `yay -S xwin`

# Requirements for vscode

In order for vscode to not show squigglies around headers like `windows.h` you need to update your `c_cpp_properties.json`. In my case I have MinGW and use the following config, just so the editor does not complain:

```json
{
    "configurations": [
        {
            "name": "Linux-MinGW",
            "includePath": [
                "${workspaceFolder}/**",
                "/usr/x86_64-w64-mingw32/include/**"
            ],
            "defines": [
                "_WIN32",
                "__WIN32__",
                "WIN32",
                "_WINDOWS"
            ],
            "compilerPath": "/usr/bin/x86_64-w64-mingw32-g++",
            "cStandard": "c17",
            "cppStandard": "c++17",
            "intelliSenseMode": "gcc-x64"
        }
    ],
    "version": 4
}
```

# Setup

## XWIN

Use xwin to get all windows headers and sdk required to replicate MSVC 2013+ behavior. I suggest setting an env var for the path to xwin cache you prefer. 

Example:
```bash
export XWIN_CACHE=$HOME/.xwin

xwin --cache-dir $XWIN_CACHE --accept-license splat --preserve-ms-arch-notation
```

## Slight patches to the ScriptHookSDK

Once you have your ScriptHookRDR2 SDK, you will see a few folders inside, `inc/` and `lib/` are the relevant ones, I suggest you keep them in your project untracked like I've done here (I have them in `SDK` but the folders are on `.gitignore`)

Open The `inc` folder and inside there are multiple headers, you need to make an edit to `types.h`

Near the end you will see the definition for `Vector3` its a 24-bytes struct, 8-byte aligned, but it won't work out of the bag on linux, because the align instruction will be ignored, you need to make it conditional.

### Here is the before:

```cpp
#define ALIGN8 __declspec(align(8))

struct Vector3
{
	ALIGN8 float x;
	ALIGN8 float y;
	ALIGN8 float z;
};
```

### This is the after:

```cpp
#ifdef __cplusplus
	#define ALIGN8 alignas(8)
#else
	#define ALIGN8 __declspec(align(8))
#endif

struct Vector3
{
	ALIGN8 float x;
	ALIGN8 float y;
	ALIGN8 float z;
};
```

What this does is define the `ALIGN8` instruction as `alignas(8)` on linux and replicate the behavior of the otherwise ignored `__declspec(align(8))`

That should be the only change you need as far as I know.

## Compilation

To replicate the behavior of MSVC 2013+, a command as follows works:

```bash
clang-cl --target=x86_64-pc-windows-msvc \
    /vctoolsdir "${XWIN_CACHE}/splat/crt" \
    /winsdkdir "${XWIN_CACHE}/splat/sdk" \
    -fuse-ld=lld \
    /LD \
    /EHsc \
    /O2 \
    <your-cpp-files> ../SDK/lib/ScriptHookRDR2.lib \ # Update the path to your ScriptHookRDR2.lib
    /link /OUT:<your-out-file-path>.asi

```

# Example

There is a simple mod that prints the count of premium cigarettes on Arthurs Satchel [here](./ExampleCigarCount/main.cpp), it includes a [`compile.sh`](./ExampleCigarCount/compile.sh) file.
