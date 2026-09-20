param(
    [string]$Configuration = "Release"
)

$ErrorActionPreference = "Stop"

$workspaceRoot = $PSScriptRoot
$srcDir = Join-Path $workspaceRoot "src"
$projFile = Join-Path $srcDir "PluginClipboard.csproj"
$releaseDir = Join-Path $workspaceRoot "Release"

# Locate MSBuild
$msbuildPaths = @(
    "C:\Windows\Microsoft.NET\Framework64\v4.0.30319\MSBuild.exe",
    "C:\Windows\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe"
)

$msbuild = $msbuildPaths | Where-Object { Test-Path $_ } | Select-Object -First 1

if (-not $msbuild) {
    Write-Error "MSBuild.exe was not found on the system."
    exit 1
}

Write-Host "Using MSBuild at: $msbuild" -ForegroundColor Cyan

# Locate ildasm / Windows SDK tools directory
$sdkToolPaths = @(
    "C:\Program Files (x86)\Microsoft SDKs\Windows\v10.0A\bin\NETFX 4.8 Tools",
    "C:\Program Files (x86)\Microsoft SDKs\Windows\v8.1A\bin\NETFX 4.5.1 Tools",
    "C:\Program Files (x86)\Microsoft SDKs\Windows\v8.0A\bin\NETFX 4.0 Tools"
)

$sdkPath = $sdkToolPaths | Where-Object { Test-Path $_ } | Select-Object -First 1
Write-Host "Using SDK Tools at: $sdkPath" -ForegroundColor Cyan

# Create Release directories
$relX86 = Join-Path $releaseDir "x86"
$relX64 = Join-Path $releaseDir "x64"
$rel32 = Join-Path $releaseDir "32bit"
$rel64 = Join-Path $releaseDir "64bit"

@($relX86, $relX64, $rel32, $rel64) | ForEach-Object {
    if (-not (Test-Path $_)) {
        New-Item -ItemType Directory -Path $_ -Force | Out-Null
    }
}

$asmName = "PluginClipboard"

Write-Host "`n================ BUILDING $asmName (x64) ================" -ForegroundColor Green
$buildArgsX64 = @(
    $projFile,
    "/p:Configuration=$Configuration",
    "/p:Platform=x64",
    "/p:AssemblyName=$asmName",
    "/t:Rebuild",
    "/p:DllExportSdkPath=$sdkPath",
    "/v:m"
)
& $msbuild $buildArgsX64
if ($LASTEXITCODE -ne 0) {
    Write-Error "x64 build of $asmName failed with exit code $LASTEXITCODE"
    exit $LASTEXITCODE
}

$x64Out = Join-Path $srcDir "bin\x64\$Configuration\$asmName.dll"
Copy-Item -Path $x64Out -Destination (Join-Path $relX64 "$asmName.dll") -Force
Copy-Item -Path $x64Out -Destination (Join-Path $rel64 "$asmName.dll") -Force

Write-Host "`n================ BUILDING $asmName (x86) ================" -ForegroundColor Green
$buildArgsX86 = @(
    $projFile,
    "/p:Configuration=$Configuration",
    "/p:Platform=x86",
    "/p:AssemblyName=$asmName",
    "/t:Rebuild",
    "/p:DllExportSdkPath=$sdkPath",
    "/v:m"
)
& $msbuild $buildArgsX86
if ($LASTEXITCODE -ne 0) {
    Write-Error "x86 build of $asmName failed with exit code $LASTEXITCODE"
    exit $LASTEXITCODE
}

$x86Out = Join-Path $srcDir "bin\x86\$Configuration\$asmName.dll"
Copy-Item -Path $x86Out -Destination (Join-Path $relX86 "$asmName.dll") -Force
Copy-Item -Path $x86Out -Destination (Join-Path $rel32 "$asmName.dll") -Force

Write-Host "`n================ BUILD COMPLETED SUCCESSFULLY ================" -ForegroundColor Cyan
