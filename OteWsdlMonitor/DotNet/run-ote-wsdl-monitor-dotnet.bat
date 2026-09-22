@echo off
setlocal EnableExtensions

cd /d "%~dp0" || (
    echo ERROR: Cannot change to the .NET monitor directory.
    exit /b 1
)

where dotnet >nul 2>&1
if errorlevel 1 (
    echo ERROR: dotnet was not found in PATH.
    exit /b 1
)

set "APP_DLL=%~dp0bin\Release\net8.0-windows\OteWsdlMonitor.dll"
if exist "%APP_DLL%" (
    dotnet "%APP_DLL%" %*
) else (
    dotnet run --project "%~dp0OteWsdlMonitor.csproj" -- %*
)
set "EXIT_CODE=%ERRORLEVEL%"

endlocal & exit /b %EXIT_CODE%
