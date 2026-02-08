@echo off
setlocal

set "ROOT=%~dp0"
set "CONFIG=Debug"
if /I "%~1"=="Release" set "CONFIG=Release"

pushd "%ROOT%" >nul

taskkill /F /IM DesktopMemo.App.exe >nul 2>&1

dotnet build DesktopMemo.sln -c %CONFIG%
if errorlevel 1 (
  popd >nul
  exit /b 1
)

set "APP_EXE=%ROOT%src\DesktopMemo.App\bin\%CONFIG%\net48\DesktopMemo.App.exe"
if not exist "%APP_EXE%" (
  echo Executable not found: "%APP_EXE%"
  popd >nul
  exit /b 1
)

start "" "%APP_EXE%"
set "EXIT_CODE=%ERRORLEVEL%"

popd >nul
exit /b %EXIT_CODE%
