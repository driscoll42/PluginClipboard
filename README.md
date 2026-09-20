# PluginClipboard for Rainmeter (v1.0.1.0)

A patched and modernized build of the Rainmeter `PluginClipboard` plugin (originally authored by Evgenii Vilkov), used by skins such as SilverAzide's **Clipboard Meter** in the **Gadgets** suite.

## What's New in v1.0.1.0

- **Crash Fix (`NullReferenceException`)**: Eliminates the JIT debugging crash dialog caused by `ClipboardData.<.ctor>b__1(DataObject o)` when copying images or formatted content from modern browsers (Chrome, Edge, Firefox), Windows Snipping Tool, or Electron apps.
- **Multi-Instance / Reload Fix**: Properly indexes history slots per measure name (`MeasureLine1`..`MeasureLine10`) or explicit `Index` / `Line` parameters, allowing multiple gadget instances to run without measure index collisions or blank slots.
- **Thread & Lifecycle Safety**: Reference counts active measures so the hidden WinForms message pump stays open until all skins unload.
- **Version Bump (1.0.1.0)**: Allows Rainmeter's Skin Packager and Installer to smoothly upgrade older 1.0.0.0 installations.

## Releases & Downloads

- `Release/PluginClipboard_v1.0.1.0.zip`: Standalone 32-bit and 64-bit DLLs for upstream maintainers.
- `Clipboard.Meter.-.Gadgets.Patch_7.4.0.rmskin`: Complete skin package bundled with the original Gadgets skin assets and the new 1.0.1.0 plugin DLLs.
- `PluginClipboard_Submission_Package.zip`: Complete developer submission archive (sources, dependencies, build scripts, and binaries).

## Building from Source

Requirements:
- Windows with .NET Framework 4.5+ and Windows SDK NETFX Tools (`ildasm.exe`).

Run the build script:
```powershell
.\build.ps1
```

Generate the release packages:
```powershell
python package_rmskin.py
```
