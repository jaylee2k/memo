# Changed

## Unreleased

### Added
- WPF 3단 레이아웃 메모 관리 UI(그룹 트리, 메모 목록, 상세 편집, 휴지통 탭) 구현.
- 메모/그룹 CRUD, 휴지통 복구/비우기/만료 정리, Sticky Note 다중 창 기능 구현.
- 알람(단일/반복/스누즈 5/10/30), 트레이 아이콘 클릭/더블클릭 동작 구현.
- Markdown 미리보기(WebBrowser + Markdig) 기능 추가.
- MSTest V2 기반 테스트 프로젝트(`tests/DesktopMemo.Tests`) 및 단위 테스트 추가.
- Inno Setup 설치 패키징 스크립트/CI 워크플로우 추가.

### Changed
- SQLite DB 초기화를 EF 마이그레이션 의존 방식에서 스키마 보장 방식(`CREATE TABLE IF NOT EXISTS`)으로 전환.
- SQLite GUID 저장 포맷을 `BinaryGUID=false`로 고정해 GUID 컬럼을 문자열(TEXT)로 일관화.
- 앱 시작 시 레거시 Blob GUID 데이터 자동 정규화(재저장) 로직 추가.
- 메모 생성 시 예외 처리 및 FK 오류 발생 시 Inbox fallback 재시도 로직 보강.

### Fixed
- `No MigrationSqlGenerator found for provider 'System.Data.SQLite'` 예외 해결.
- `FOREIGN KEY constraint failed`(Notes insert 시 GroupId FK 실패) 문제 해결.
- 실행 중 파일 잠금으로 인한 Debug 빌드 실패 상황에서 Release 빌드/테스트 경로 정리.

## Notes
- Log updates here before creating a release.
