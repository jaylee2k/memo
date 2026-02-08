# Repository Guidelines

## Project Structure & Module Organization
- `src/DesktopMemo.App`: WPF UI (`App.xaml`, windows, view models, app services).
- `src/DesktopMemo.Domain`: core entities, DTO contracts, enums, and service interfaces.
- `src/DesktopMemo.Data`: SQLite persistence, DB bootstrap/seeding, and path providers.
- `src/DesktopMemo.Services`: business logic implementations and mapping helpers.
- `tests/DesktopMemo.Tests`: MSTest unit tests grouped by layer (`Data/`, `Domain/`, `Services/`).
- `scripts/`: build/packaging automation (notably `package-inno.ps1`).
- `deploy/inno/`: Inno Setup installer definition.
- `artifacts/` and `TestResults/`: generated outputs; do not commit build artifacts.

## Build, Test, and Development Commands
Run from repository root:
- `dotnet restore DesktopMemo.sln`: restore NuGet packages.
- `dotnet build DesktopMemo.sln -c Debug`: compile all projects for local development.
- `dotnet build DesktopMemo.sln -c Release`: compile when Debug outputs are locked by a running IDE process.
- `dotnet test tests/DesktopMemo.Tests/DesktopMemo.Tests.csproj -c Debug`: run MSTest suite.
- `dotnet test tests/DesktopMemo.Tests/DesktopMemo.Tests.csproj -c Release`: stable CI-aligned test run.
- `./scripts/package-inno.ps1 -Configuration Release -Version 1.0.0`: build and package installer.
- `./scripts/package-inno.ps1 -Configuration Release -SkipIscc`: stage release files without Inno Setup.

## Coding Style & Naming Conventions
- Language: C# (`LangVersion=latest`) on .NET Framework 4.8.
- Use 4-space indentation and file-scoped namespaces (`namespace X;`).
- Naming: `PascalCase` for types/methods/properties, `camelCase` for locals/parameters, `_camelCase` for private fields.
- Keep one public type per file; file names should match the main type (for example, `NoteService.cs`).
- Preserve architectural boundaries: App -> Services -> Data/Domain.

## Testing Guidelines
- Framework: MSTest (`Microsoft.NET.Test.Sdk`, `MSTest.TestFramework`).
- Place tests in `tests/DesktopMemo.Tests/...` mirroring source structure.
- Test files end with `Tests.cs`; method names follow `Method_Scenario_Expected`.
- Prefer deterministic tests and cover business rules in `DesktopMemo.Services` first.
- Tests run with method-level parallelization configured in `tests/DesktopMemo.Tests/MSTestSettings.cs`.

## Commit & Pull Request Guidelines
- The repository currently has no commit history on `main`; use clear, imperative commit subjects.
- Recommended format: `<area>: <change>` (example: `services: validate snooze minute range`).
- Keep commits focused and atomic; avoid mixing refactors with feature fixes.
- PRs should include: concise summary, test evidence (`dotnet test` output), linked issue/task, and screenshots for UI changes.

## Security & Configuration Tips
- Runtime SQLite DB is stored under `%LOCALAPPDATA%\\DesktopMemo\\desktopmemo.db`.
- Do not commit local DB files, logs, or installer outputs.
- If Inno Setup is not installed locally, use `-SkipIscc` and validate staged output in `artifacts/staging/net48`.
- DB bootstrap guarantees required tables and seed rows (`Inbox`, global font settings) on startup.
- Legacy SQLite rows with Blob GUID values are normalized at startup to prevent FK failures.
