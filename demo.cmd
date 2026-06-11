@echo off
REM Double-click this file on Windows to run the demo.
REM Requires the .NET 8 SDK. The window stays open at the end so you can read
REM the results (the program pauses for Enter). To build a standalone demo.exe
REM that needs no SDK, run scripts\build-demo.ps1 instead.
cd /d "%~dp0"
echo Building and running the ATE-framework-lite demo...
echo.
dotnet run --project AteFramework.Lite.csproj -- profiles/lna_plan.csv
if errorlevel 1 (
  echo.
  echo The demo exited with an error. Is the .NET 8 SDK installed?
  echo Download: https://dotnet.microsoft.com/download/dotnet/8.0
  pause
)
