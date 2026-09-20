import os
import shutil
import struct
import zipfile

def build_all_packages():
    pkg_dir = "rmskin_build"
    if os.path.exists(pkg_dir):
        shutil.rmtree(pkg_dir)
    os.makedirs(pkg_dir, exist_ok=True)

    # 1. Official RMSKIN.ini from skin_source
    shutil.copyfile(os.path.join("skin_source", "RMSKIN.ini"), os.path.join(pkg_dir, "RMSKIN.ini"))

    # 2. Plugins (1.0.1.0 PluginClipboard.dll for 32-bit and 64-bit)
    plugins_32 = os.path.join(pkg_dir, "Plugins", "32bit")
    plugins_64 = os.path.join(pkg_dir, "Plugins", "64bit")
    os.makedirs(plugins_32, exist_ok=True)
    os.makedirs(plugins_64, exist_ok=True)

    shutil.copyfile("Release/x86/PluginClipboard.dll", os.path.join(plugins_32, "PluginClipboard.dll"))
    shutil.copyfile("Release/x64/PluginClipboard.dll", os.path.join(plugins_64, "PluginClipboard.dll"))

    # 3. Copy original skin assets from skin_source
    skins_src = os.path.join("skin_source", "Skins")
    skins_dst = os.path.join(pkg_dir, "Skins")
    shutil.copytree(skins_src, skins_dst)

    # 4. Create RMSKIN archive with official 16-byte footer
    zip_temp = "temp_rmskin.zip"
    if os.path.exists(zip_temp):
        os.remove(zip_temp)

    with zipfile.ZipFile(zip_temp, "w", compression=zipfile.ZIP_DEFLATED) as z:
        for root, dirs, files in os.walk(pkg_dir):
            for file in files:
                abs_path = os.path.join(root, file)
                rel_path = os.path.relpath(abs_path, pkg_dir).replace("\\", "/")
                z.write(abs_path, rel_path)

    with open(zip_temp, "rb") as f:
        zip_bytes = f.read()

    zip_size = len(zip_bytes)
    footer = struct.pack("<Q", zip_size) + b"\x00RMSKIN\x00"
    final_bytes = zip_bytes + footer

    output_rmskin_names = [
        "Clipboard.Meter.-.Gadgets.Patch_7.4.0.rmskin",
        "Release/Clipboard.Meter.-.Gadgets.Patch_7.4.0.rmskin"
    ]

    for out_name in output_rmskin_names:
        os.makedirs(os.path.dirname(os.path.abspath(out_name)), exist_ok=True)
        with open(out_name, "wb") as f:
            f.write(final_bytes)
        print(f"Created {out_name} ({len(final_bytes)} bytes)")

    # Cleanup temp
    os.remove(zip_temp)
    shutil.rmtree(pkg_dir)

    # 5. Build Standalone Plugin Binaries Package (PluginClipboard_v1.0.1.0.zip)
    # This package contains only the plugin DLLs and docs (ideal for upstream maintainers)
    plugin_zip_names = [
        "PluginClipboard_v1.0.1.0.zip",
        "Release/PluginClipboard_v1.0.1.0.zip"
    ]
    for p_zip in plugin_zip_names:
        if os.path.exists(p_zip):
            os.remove(p_zip)
        with zipfile.ZipFile(p_zip, "w", compression=zipfile.ZIP_DEFLATED) as z:
            z.write("Release/x86/PluginClipboard.dll", "x86/PluginClipboard.dll")
            z.write("Release/x64/PluginClipboard.dll", "x64/PluginClipboard.dll")
            z.write("Release/x86/PluginClipboard.dll", "32bit/PluginClipboard.dll")
            z.write("Release/x64/PluginClipboard.dll", "64bit/PluginClipboard.dll")
            if os.path.exists("README.md"):
                z.write("README.md", "README.md")
            if os.path.exists("PullRequest_Notes.md"):
                z.write("PullRequest_Notes.md", "PullRequest_Notes.md")
        print(f"Created {p_zip} ({os.path.getsize(p_zip)} bytes)")

    # 6. Build Developer Submission ZIP
    sub_zip_name = "PluginClipboard_Submission_Package.zip"
    if os.path.exists(sub_zip_name):
        os.remove(sub_zip_name)

    with zipfile.ZipFile(sub_zip_name, "w", compression=zipfile.ZIP_DEFLATED) as z:
        for folder in ["src", "packages", "Release"]:
            for root, dirs, files in os.walk(folder):
                parts = root.split(os.sep)
                if "bin" in parts or "obj" in parts or "__pycache__" in parts:
                    continue
                for file in files:
                    abs_path = os.path.join(root, file)
                    z.write(abs_path, abs_path)
        z.write("build.ps1", "build.ps1")
        if os.path.exists("PullRequest_Notes.md"):
            z.write("PullRequest_Notes.md", "PullRequest_Notes.md")
        if os.path.exists("README.md"):
            z.write("README.md", "README.md")
        if os.path.exists("Clipboard.Meter.-.Gadgets.Patch_7.4.0.rmskin"):
            z.write("Clipboard.Meter.-.Gadgets.Patch_7.4.0.rmskin", "Clipboard.Meter.-.Gadgets.Patch_7.4.0.rmskin")

    print(f"Created {sub_zip_name} ({os.path.getsize(sub_zip_name)} bytes)")
    print("All packages built successfully!")

if __name__ == "__main__":
    build_all_packages()
