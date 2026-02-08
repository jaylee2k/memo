# DesktopMemo Packaging

## Inno Setup Installer

Prerequisites:
- .NET SDK installed
- Inno Setup 6 installed (`ISCC.exe` in PATH), unless using `-SkipIscc`

Build an installer locally:

```powershell
./scripts/package-inno.ps1 -Configuration Release -Version 1.0.0
```

If Inno Setup is not installed, stage files only:

```powershell
./scripts/package-inno.ps1 -Configuration Release -SkipIscc
```

Output locations:
- Staged app files: `artifacts/staging/net48`
- Installer exe: `artifacts/installer`

## Troubleshooting

- If `ISCC.exe not found` appears, install Inno Setup 6 or run with `-SkipIscc`.
- If Debug build output is locked by Visual Studio/running app, use:

```powershell
dotnet build DesktopMemo.sln -c Release
```

## CI

GitHub Actions workflow file:
- `.github/workflows/build-installer.yml`

The workflow installs Inno Setup, builds the app, produces installer, and uploads it as an artifact.
