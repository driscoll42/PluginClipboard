# Fix NullReferenceException & Multi-Instance Support for PluginClipboard.dll (v1.0.1.0)

## Summary
This patch resolves a crashing `NullReferenceException` in `PluginClipboard.dll` (used by SilverAzide's Clipboard Meter skin in the Gadgets suite) when users copy images or formatted content to the Windows clipboard from modern applications (such as Chromium/Gecko browsers, Windows Snipping Tool, Electron apps, or image editors).

In addition, it resolves a multi-instance bug where running multiple skins or instances simultaneously (or refreshing skins) broke measure index mapping and caused items to only appear in one skin.

The assembly version and file version have been bumped to **1.0.1.0** in `Properties\AssemblyInfo.cs` so that the Rainmeter installer / skin packager will automatically upgrade existing `1.0.0.0` installations.

---

## Crash Description & Stack Trace
When copying certain image formats from modern web browsers (Chrome, Edge, Firefox) or Windows Snipping Tool, Windows places synthetic or proprietary clipboard formats onto the clipboard. WinForms `DataObject.GetImage()` or `IDataObject.GetData(DataFormats.Bitmap)` can return `null` if GDI+ cannot decode the format directly or if format conversion is unsupported.

The original plugin directly accessed `.Size` on the returned `Image` object without a null guard:

```text
************** Exception Text **************
System.NullReferenceException: Object reference not set to an instance of an object.
   at PluginClipboard.ClipboardData.<.ctor>b__1(DataObject o)
   at PluginClipboard.ClipboardData..ctor(IDataObject dataObject)
   at PluginClipboard.ClipboardViewer.WndProc(Message& m)
   at System.Windows.Forms.NativeWindow.Callback(IntPtr hWnd, Int32 msg, IntPtr wparam, IntPtr lparam)
```

---

## Root Cause Analysis & Fixes

### 1. `NullReferenceException` in Image Extraction (`ClipboardData.cs`)
- **Original issue**: In `ClipboardData._convertors[DataFormats.Bitmap]`, lambda `(DataObject o) => "[IMG] " + o.GetImage().Size` called `.Size` on `o.GetImage()`. When `o.GetImage()` returned `null`, a `NullReferenceException` was thrown inside the hidden WinForms message loop (`WndProc`), triggering JIT debugger dialogs or crashing Rainmeter.
- **Fix**: Safely inspects `o` and `o.GetImage()`. If `GetImage()` returns `null` or throws a GDI+ decoding error, it catches the exception and returns `null` so the plugin falls back gracefully to timestamped generic data (`[DATA] <Time>`).
- **FileDrop null guard**: Verifies `GetFileDropList()` is non-null and contains at least one item before indexing `[0]`.
- **Defensive format iteration**: Wrapped format conversion and `dataObject.GetData(format)` with exception guards against COM clipboard lock contention.
- **Added UnicodeText and StringFormat**: Guarantees standard Unicode text clipboard entries are consistently read.

### 2. Multi-Instance Measure Indexing Bug (`Measure.cs`)
- **Original issue**: In the original decompiled code, measures used a global static counter `_id = Count; Count++` in their constructor. When multiple skins/instances loaded, subsequent instances received measure IDs 10..19 (which were out of range of the 10-item history list and stayed blank). When a skin unloaded, `Finalize` reset `Count = 0` and killed the clipboard viewer for all instances.
- **Fix**: In `Measure.Reload`, the slot index is determined by:
  1. Reading an explicit `Index`, `Line`, or `Item` parameter in the `.ini` (if present).
  2. Parsing the numeric suffix of the measure name (e.g., `MeasureLine1` -> 0, `MeasureLine2` -> 1 ... `MeasureLine10` -> 9).
- **Safe Lifecycle**: Active measures are reference-counted with `Interlocked.Increment` / `Decrement`. `ClipboardViewer.Stop()` is only called when all active measures across all skins have finalized.

### 3. Assembly Version Upgrade (`Properties\AssemblyInfo.cs`)
- Updated `AssemblyVersion` and `AssemblyFileVersion` to `"1.0.1.0"`.
- This ensures the Rainmeter installer / skin packager recognizes the DLLs as an upgrade over version 1.0.0.0 and overwrites older files during installation.

---

## Deliverables

- **Plugin Binaries (`Release/`)**:
  - `Release/x64/PluginClipboard.dll` (64-bit native C-exports, FileVersion 1.0.1.0)
  - `Release/x86/PluginClipboard.dll` (32-bit native C-exports, FileVersion 1.0.1.0)
  - `Release/PluginClipboard_v1.0.1.0.zip` (standalone archive containing both DLLs and documentation)

---

## Verified Native Unmanaged Exports
Both 32-bit and 64-bit binaries export the standard 6 Rainmeter plugin entry points:
- `Initialize` (ordinal 0)
- `Finalize` (ordinal 1)
- `Reload` (ordinal 2)
- `Update` (ordinal 3)
- `GetString` (ordinal 4)
- `ExecuteBang` (ordinal 5)

---

## Build Instructions

To build the plugin DLLs from source:
```powershell
.\build.ps1
```
