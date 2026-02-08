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
- If Debug build output is locked by a running app/IDE, use:

```powershell
dotnet build DesktopMemo.sln -c Release
```

## Notes
- Packaging is script-driven via `scripts/package-inno.ps1`.
- No repository-local CI workflow file is currently checked in for installer automation.
