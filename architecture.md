# DesktopMemo Architecture

## 1. Scope
- This document describes the current architecture implemented in the source code under `src/` and `tests/`.
- Focus areas:
- Solution/module boundaries
- Runtime composition and execution flows
- Persistence model and data lifecycle
- UI/ViewModel orchestration
- Test coverage and known architectural tradeoffs

## 2. System Overview
- DesktopMemo is a .NET Framework 4.8 WPF desktop app for memo management with:
- Hierarchical memo groups
- Markdown editing and preview
- Alarm scheduling (single/repeating + snooze)
- Sticky-note popout windows
- Soft delete + trash with retention purge
- Tray icon and background workers
- Persistence is SQLite (EF6 + direct SQL bootstrap) under `%LOCALAPPDATA%\DesktopMemo\desktopmemo.db` by default.

## 3. Solution and Module Boundaries

### 3.1 Projects
- `DesktopMemo.App`
- WPF UI, ViewModels, app lifecycle, tray integration, sticky window orchestration.
- `DesktopMemo.Domain`
- Entities, DTO contracts, enums, request objects, service interfaces.
- `DesktopMemo.Data`
- EF6 `DbContext`, connection/path handling, schema bootstrap, seed data.
- `DesktopMemo.Services`
- Business logic implementations (`IGroupService`, `INoteService`, `ITrashService`, `ISettingsService`, `IAlarmService`) and mappers.
- `DesktopMemo.Tests`
- MSTest regression/unit tests across App/Data/Domain/Services.

### 3.2 Dependency Direction (compile-time)
```text
DesktopMemo.App
  -> DesktopMemo.Services
  -> DesktopMemo.Data
  -> DesktopMemo.Domain

DesktopMemo.Services
  -> DesktopMemo.Data
  -> DesktopMemo.Domain

DesktopMemo.Data
  -> DesktopMemo.Domain

DesktopMemo.Domain
  -> (no project references)
```

### 3.3 Architectural Intent vs Current Reality
- Intended layering is App -> Services -> Data/Domain.
- Current implementation has deliberate App -> Data usage in two places:
- `App.xaml.cs` directly calls `DatabaseBootstrapper.Initialize()`.
- `StickyNoteWindowService` depends on `IDbContextFactory` for sticky window state persistence.
- Result: mostly layered architecture with a few pragmatic boundary shortcuts.

## 4. Runtime Composition (DI and Lifetimes)

### 4.1 DI Root
- DI root is `App.OnStartup`.
- `AddDesktopMemoCore()` registers:
- `IDbContextFactory` as singleton.
- `IGroupService`, `INoteService`, `ITrashService`, `ISettingsService` as singleton.
- App layer adds:
- `IStickyNoteService -> StickyNoteWindowService` (singleton).
- `IAlarmService -> AlarmService` (singleton).
- `IAppLifecycleService -> AppLifecycleService` (singleton).
- UI services (`TrayIconService`, `AlarmBackgroundWorker`) as singleton.
- `MainViewModel`, `MainWindow` as singleton.
- `SettingsViewModel`, `SettingsWindow` as transient.

### 4.2 Why singleton services work here
- Service classes are effectively stateless and create a new `DesktopMemoDbContext` per method call via `IDbContextFactory`.
- This avoids long-lived EF context issues while keeping service registration simple.

## 5. Startup and Shutdown Flow

### 5.1 Startup
1. Acquire single-instance mutex (`DesktopMemo.SingleInstance`).
2. Run `DatabaseBootstrapper.Initialize()`:
- Ensure tables/indexes exist (raw SQL `CREATE TABLE IF NOT EXISTS`).
- Normalize legacy GUID blob rows to text storage.
- Ensure seed data (`Inbox` group + global font settings).
3. Build DI container.
4. Create tray icon service and alarm background worker.
5. Start worker timers and execute immediate alarm/purge pass.
6. Resolve `MainWindow` as `Application.MainWindow`.

### 5.2 Shutdown
- Dispose background worker, tray icon, DI container.
- Release mutex.
- Main window close is intercepted and hidden unless explicit app exit (`AppState.IsExiting`).

## 6. UI Architecture

### 6.1 Main UI
- `MainWindow.xaml` is tab-based:
- Memo tab: group tree, searchable note list, detail editor, alarm/move/delete action bar.
- Trash tab: deleted items grid + restore/purge/empty actions.
- Status bar panel at bottom via `StatusMessage`.
- Markdown preview uses `WebBrowser` and `Markdig`-generated HTML.

### 6.2 ViewModel responsibilities
- `MainViewModel` orchestrates:
- Group tree load/selection.
- Note list load/filter (`FilteredNotesView`, `SearchKeyword`).
- Note editor state and auto-save via `Debouncer` (500ms).
- Alarm and trash commands.
- Sticky window open requests.
- `SettingsViewModel` edits global default font settings.
- `StickyNoteViewModel` provides lightweight edit surface with debounced saves.

### 6.3 App services in UI layer
- `AppLifecycleService`: show/focus main/settings windows and controlled shutdown.
- `TrayIconService`: tray menu + click handlers.
- `UserDialogService`: prompt/confirm/error/info abstraction over modal dialogs.

## 7. Core Business Flows

### 7.1 Group lifecycle
- Create/update/soft-delete groups in `GroupService`.
- Soft delete cascades to descendant groups and all notes in subtree.
- `Inbox` is protected from deletion at ViewModel level.

### 7.2 Note lifecycle
- Create notes with defaults from global font settings.
- Update note content/style/alarm/repeat metadata.
- Move between groups.
- Soft delete into trash.
- Special defensive path:
- If note create hits FK failure (legacy broken DB state), fallback to active `Inbox`.

### 7.3 Alarm lifecycle
- `AlarmBackgroundWorker` runs:
- Every 30s: `IAlarmService.ProcessDueAlarms()`
- Every 6h: `ITrashService.PurgeExpiredItems()`
- `AlarmService` behavior:
- Effective trigger = `SnoozeUntilUtc` first, otherwise `AlarmAtUtc`.
- On due alarm: toast notification + open sticky note + update next trigger for repeats.
- Snooze supports only 5/10/30 minutes (validated).
- Dismiss disables non-repeat alarms, advances repeat alarms to next occurrence.

### 7.4 Sticky note lifecycle
- `StickyNoteWindowService` maintains open sticky windows keyed by `NoteId`.
- Reuses existing window if already open.
- Persists window bounds/topmost/last-opened in `StickyWindowStates`.
- Restores previous geometry/topmost on open.

### 7.5 Trash lifecycle
- Soft-deleted notes/groups are visible in trash.
- Restore note rebinds to active group or Inbox fallback.
- Permanent delete:
- Note: delete note + related sticky state.
- Group: only allowed when subtree has no active groups/notes.
- Retention purge removes deleted items older than 90 days.

## 8. Persistence Architecture

### 8.1 Storage
- SQLite connection uses:
- `BinaryGUID=false` (GUID as text)
- Foreign keys enabled
- WAL journal mode
- Normal sync mode
- DB path can be overridden by env var `DESKTOPMEMO_DB_PATH`.

### 8.2 Data model
- `MemoGroups`
- Self-referencing hierarchy (`ParentGroupId`), soft delete columns.
- `Notes`
- FK to group, markdown + style settings + alarm/repeat fields + soft delete columns.
- `StickyWindowStates`
- One-to-one with note (`NoteId` PK/FK).
- `AppSettings`
- Key-value store for global settings.

### 8.3 Schema management strategy
- No EF migrations.
- Schema and indexes are enforced in startup bootstrap SQL.
- Seeder is idempotent (`INSERT ... WHERE NOT EXISTS`, `INSERT OR IGNORE`).
- Legacy GUID normalization rewrites BLOB GUID rows into text representation.

## 9. Cross-Cutting Concerns
- Single-instance safety via named mutex.
- Soft-delete-first strategy for data safety.
- Debounced writes to reduce save pressure from text editing.
- Local-time UI with UTC persistence for alarm/date fields.
- Basic exception-to-dialog handling in ViewModels for user-facing operations.

## 10. Testing Architecture

### 10.1 Test stack
- MSTest (`Microsoft.NET.Test.Sdk`, `MSTest.TestAdapter`, `MSTest.TestFramework`).
- Assembly-level `[DoNotParallelize]` is enabled in `MSTestSettings.cs`.

### 10.2 Coverage snapshot
- Domain defaults:
- Validates default constructor values for `Note` and `FontSettingDto`.
- Data bootstrap/path:
- Verifies DB path shape and directory creation.
- Regression tests bootstrap idempotency, schema presence, GUID text storage, FK integrity.
- Services:
- `EntityMapper` mapping defaults and descendant collection.
- `AlarmCalculator` trigger/recurrence behavior.
- App/ViewModel:
- Regression for `MainViewModel.AddNoteCommand` exception handling path.

### 10.3 Notable test gap areas
- End-to-end UI command flows in `MainViewModel` beyond add-note failure case.
- AlarmService integration with real DB and sticky service interactions.
- TrashService edge cases around deep nested group deletion order.

## 11. Build and Packaging Architecture
- Build: `dotnet build DesktopMemo.sln -c Debug|Release`.
- Packaging script: `scripts/package-inno.ps1`.
- Stages app output to `artifacts/staging/net48`.
- Optionally compiles Inno Setup installer (`deploy/inno/DesktopMemo.iss`) to `artifacts/installer`.
- Can run in staging-only mode with `-SkipIscc`.

## 12. Architectural Tradeoffs and Risks
- App layer has direct Data dependency for bootstrap and sticky window persistence.
- This simplifies implementation but weakens strict layer isolation.
- Service registrations are singleton, which is safe only while services remain stateless.
- `MainViewModel` is a large orchestrator and may become hard to evolve without decomposition.
- Schema evolution is startup-SQL based, so complex future migrations may require careful manual scripts.

