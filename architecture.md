# DesktopMemo Architecture

## 1. Scope
- This document summarizes the current architecture in `src/` and `tests/`.
- Last updated: 2026-02-08

## 2. System Overview
- DesktopMemo is a .NET Framework 4.8 WPF desktop memo app.
- Core capabilities:
- Hierarchical group management
- Markdown note editing + preview
- Alarm scheduling (single/repeat + snooze)
- Sticky note pop-out windows
- Soft delete + trash retention purge
- SQLite local persistence

## 3. Project Structure
- `src/DesktopMemo.App`
- Presentation layer (WPF windows, ViewModels, UI services, lifecycle/tray/background worker).
- `src/DesktopMemo.Domain`
- Entities, DTOs, enums, request contracts, service interfaces.
- `src/DesktopMemo.Data`
- EF6 `DbContext`, SQLite connection/path, schema bootstrap, seed and legacy GUID normalization.
- `src/DesktopMemo.Services`
- Business implementations and mapping utilities.
- `tests/DesktopMemo.Tests`
- MSTest coverage for App/Data/Domain/Services.

## 4. Dependency Direction
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
```

Notes:
- Intended boundary is App -> Services -> Data/Domain.
- App currently references Data directly for startup bootstrap and sticky window state persistence path.

## 5. Runtime Composition

### 5.1 Startup
1. Single-instance mutex check (`DesktopMemo.SingleInstance`).
2. `DatabaseBootstrapper.Initialize()` executes:
- schema creation (`CREATE TABLE IF NOT EXISTS`)
- index creation
- legacy GUID blob normalization
- seed rows (`Inbox`, global font settings)
3. DI container build.
4. Tray icon and alarm background worker start.
5. Main window resolve.

### 5.2 Dependency Injection
- Core registrations (`AddDesktopMemoCore`):
- `IDbContextFactory`
- `IGroupService`, `INoteService`, `ITrashService`, `ISettingsService`
- App registrations:
- `IStickyNoteService`, `IAlarmService`, `IAppLifecycleService`
- `MainViewModel`, `MainWindow`, `SettingsViewModel`, `SettingsWindow`
- `TrayIconService`, `AlarmBackgroundWorker`

### 5.3 Shutdown
- Dispose worker/tray/service provider.
- Release mutex.
- Main window hides instead of closing unless explicit exit (`AppState.IsExiting`).

## 6. UI Architecture

### 6.1 MainWindow
- Dark card-based management UI.
- Sections:
- Group tree panel
- Searchable note list (`SearchKeyword`, `FilteredNotesView`)
- Detail editor (title/content/font/alarm)
- Trash tab
- Status panel
- Input validation surfaces:
- `FontSize`, `FontColorHex`, `AlarmTimeText`
- validation errors shown via red border + tooltip

### 6.2 SettingsWindow
- Custom dark settings shell with top bar.
- Maintains existing settings bindings:
- `FontFamily`, `FontSize`, `FontWeight`, `FontStyle`, `FontColorHex`, `IsUnderline`
- Validation-enabled save flow through `SettingsViewModel`.

### 6.3 StickyNoteWindow
- Custom note style UI:
- yellow top bar, dark content body, bottom toolbar-like actions
- no-titlebar chrome (`WindowStyle=None`) + drag on top bar
- window state persisted through `IStickyNoteService`.

## 7. ViewModel and Interaction Pattern
- MVVM is used with `CommunityToolkit.Mvvm`.
- `MainViewModel`:
- orchestrates group/note/trash flows
- debounced autosave (`Debouncer`, 500ms)
- markdown HTML generation
- validation (`IDataErrorInfo`)
- command handling for CRUD/alarm/trash actions
- `SettingsViewModel`:
- global font settings read/write + validation
- `StickyNoteViewModel`:
- lightweight text editing + debounced save

## 8. Business Layer Highlights
- `GroupService`: hierarchy CRUD and subtree soft-delete.
- `NoteService`: create/update/move/soft-delete, plus Inbox fallback on FK insert failure.
- `TrashService`: restore/permanent delete/purge/empty logic with leaf-first group deletion.
- `AlarmService`: due-alarm scan, toast notify, sticky open, snooze and repeat progression.
- `SettingsService`: app setting key-value upsert/read.

## 9. Persistence Model

### 9.1 Storage
- SQLite with:
- `BinaryGUID=false` (GUID as text)
- FK enabled
- WAL mode
- configurable path via `DESKTOPMEMO_DB_PATH`

### 9.2 Tables
- `MemoGroups`
- `Notes`
- `StickyWindowStates`
- `AppSettings`

### 9.3 Strategy
- No EF migration pipeline.
- Startup bootstrap ensures schema/indexes + seed state.
- Includes legacy data normalization for GUID storage format consistency.

## 10. Testing Status
- Framework: MSTest.
- Current test suite result baseline:
- Total 20, Passed 18, Skipped 2.
- Current test areas:
- Domain defaults
- Data path/bootstrap regression
- mapper and alarm calculator behavior
- ViewModel regression and validation behavior
- Assembly-level `[DoNotParallelize]` is configured.

## 11. Current Architectural Risks
- Repeat alarm recurrence still based on current UTC rather than original scheduled time baseline.
- Trash permanent-delete exception path is not fully guarded in UI command layer.
- Mutex release path can be unsafe when current process does not own mutex.
- GUID normalization rewrite path is not transaction-protected end-to-end.
- Background timers run DB-bound work on UI dispatcher thread.

