@echo off
setlocal EnableExtensions

cd /d "%~dp0" || (
    echo ERROR: Cannot change to the monitor directory.
    exit /b 1
)

rem Edit the SMTP and recipient values below for your environment.
rem Keep OTE_SMTP_PASSWORD in the Windows environment, not in this file.
set "PYTHON_EXE=python"
set "OTE_SMTP_HOST=smtp.office365.com"
set "OTE_SMTP_PORT=587"
set "OTE_SMTP_USERNAME=your-mailbox@example.com"
set "OTE_SMTP_FROM=%OTE_SMTP_USERNAME%"
set "OTE_SMTP_STARTTLS=1"
set "OTE_SMTP_SSL=0"
set "OTE_RECIPIENT=pavel.balas@seyfor.com"

if not exist "%~dp0ote_wsdl_monitor.py" (
    echo ERROR: ote_wsdl_monitor.py was not found next to this BAT file.
    exit /b 1
)

where "%PYTHON_EXE%" >nul 2>&1
if errorlevel 1 (
    echo ERROR: Python executable was not found: %PYTHON_EXE%
    exit /b 1
)

"%PYTHON_EXE%" "%~dp0ote_wsdl_monitor.py" --recipient "%OTE_RECIPIENT%" %*
set "EXIT_CODE=%ERRORLEVEL%"

endlocal & exit /b %EXIT_CODE%