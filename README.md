# PluginClipboard for Rainmeter (v1.0.1.0)

A patched and modernized build of the Rainmeter `PluginClipboard` plugin (originally authored by Evgenii Vilkov), used by skins such as SilverAzide's **Clipboard Meter** in the **Gadgets** suite.

## What's New in v1.0.1.0

- **Crash Fix (`NullReferenceException`)**: Eliminates the JIT debugging crash dialog caused by `ClipboardData.<.ctor>b__1(DataObject o)` when copying images or formatted content from modern browsers (Chrome, Edge, Firefox), Windows Snipping Tool, or Electron apps.
- **Multi-Instance / Reload Fix**: Properly indexes history slots per measure name (`MeasureLine1`..`MeasureLine10`) or explicit `Index` / `Line` parameters, allowing multiple gadget instances to run without measure index collisions or blank slots.
- **Thread & Lifecycle Safety**: Reference counts active measures so the hidden WinForms message pump stays open until all skins unload.
- **Version Bump (1.0.1.0)**: Allows Rainmeter's Skin Packager and Installer to smoothly upgrade older 1.0.0.0 installations.

## Binaries

Precompiled binaries are located in the `Release/` directory:
- `Release/x64/PluginClipboard.dll` (64-bit, v1.0.1.0)
- `Release/x86/PluginClipboard.dll` (32-bit, v1.0.1.0)
- `Release/PluginClipboard_v1.0.1.0.zip` (standalone archive containing both DLLs)

## Building from Source

### Requirements
- Windows with .NET Framework 4.5+
- Windows SDK NETFX Tools (`ildasm.exe` - included with Visual Studio or Windows SDK)

### Build Command
Run the build script in PowerShell:
```powershell
.\build.ps1
```
This compiles both 32-bit (x86) and 64-bit (x64) binaries with unmanaged entry point exports and outputs them to the `Release/` directory.

---

## Acknowledgments

This patch and modernization build was developed using Google's **Antigravity** with **Gemini 3.8 Flash (High)**.
