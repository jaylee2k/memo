# Errors

## Open Errors
| Date | Area | Error | Status | Notes |
| --- | --- | --- | --- | --- |
| - | - | - | - | 현재 추적 중인 오픈 오류 없음 |

## Resolved Errors
| Date | Area | Error | Resolution |
| --- | --- | --- | --- |
| 2026-02-08 | Data / EF6(SQLite) | `No MigrationSqlGenerator found for provider 'System.Data.SQLite'` | DB 초기화를 EF migration 경로에서 SQLite 직접 스키마 보장 경로로 전환 |
| 2026-02-08 | Data / SQLite | `FOREIGN KEY constraint failed` on `INSERT INTO Notes` | SQLite GUID 저장 포맷 `BinaryGUID=false` 고정 + 레거시 Blob GUID 자동 정규화 + Inbox fallback |
| 2026-02-08 | App / UX 안정성 | 메모 생성 실패 시 예외 전파로 앱 종료 가능 | `MainViewModel.AddNote()` 예외 처리 및 사용자 메시지 처리 추가 |
