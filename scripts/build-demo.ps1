# Build a standalone, self-contained demo.exe (no .NET runtime needed to run it).
# Output: dist/demo.exe  (plus dist/profiles/lna_plan.csv next to it).
#
# Usage (from anywhere):
#   powershell -ExecutionPolicy Bypass -File scripts\build-demo.ps1
$ErrorActionPreference = "Stop"
$root = Join-Path $PSScriptRoot ".."
Set-Location $root

$out = Join-Path $root "dist"
Write-Host "Publishing standalone demo.exe to $out ..." -ForegroundColor Cyan

dotnet publish AteFramework.Lite.csproj `
  --configuration Release `
  --runtime win-x64 `
  --self-contained true `
  -p:PublishSingleFile=true `
  -p:AssemblyName=demo `
  -p:IncludeNativeLibrariesForSelfExtract=true `
  --output $out

$exe = Join-Path $out "demo.exe"
if (Test-Path $exe) {
  Write-Host ""
  Write-Host "Done. Double-click this to run the demo:" -ForegroundColor Green
  Write-Host "  $exe"
} else {
  throw "Build finished but demo.exe was not found in $out"
}
