@echo off
setlocal

set "ROOT=%~dp0"
pushd "%ROOT%" >nul

dotnet restore DesktopMemo.sln
if errorlevel 1 (
  popd >nul
  exit /b 1
)

dotnet build DesktopMemo.sln -c Debug
set "EXIT_CODE=%ERRORLEVEL%"

popd >nul
exit /b %EXIT_CODE%
