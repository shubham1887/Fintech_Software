@echo off
REM ===========================================================================
REM  Fino - setup and upgrade.
REM
REM  This is the only file anyone needs to run. It asks Windows for
REM  administrator rights and hands over to Setup.ps1, which decides for itself
REM  whether this server needs a fresh installation or an upgrade.
REM
REM  -ExecutionPolicy Bypass applies to this one run only; no machine setting
REM  is changed.
REM ===========================================================================

setlocal
cd /d "%~dp0"

net session >nul 2>&1
if %errorLevel% neq 0 (
    echo.
    echo   Administrator rights are needed.
    echo   Asking Windows for permission...
    echo.
    powershell -NoProfile -Command "Start-Process -FilePath '%~f0' -Verb RunAs"
    exit /b
)

if not exist "%~dp0app\FintechApp.exe" (
    echo.
    echo   The application files are missing.
    echo.
    echo   Unzip the WHOLE release folder first, then run Setup.bat from inside
    echo   the unzipped folder. Running it from inside the .zip does not work:
    echo   Windows only unpacks the one file you double-clicked.
    echo.
    pause
    exit /b 1
)

powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0Setup.ps1" %*

echo.
echo   Press any key to close this window.
pause >nul
