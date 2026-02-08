# Changed

## Unreleased

### Added
- 메모 제목 검색/필터(`SearchKeyword`, `FilteredNotesView`) 기능 추가.
- 메인 편집 폼 입력 검증(`FontSize`, `FontColorHex`, `AlarmTimeText`) 및 저장 차단 로직 추가.
- 설정 창 입력 검증(`FontSize`, `FontColorHex`) 및 저장 차단 로직 추가.
- 설정/메모 입력 validation UI(오류 보더 + 툴팁) 추가.
- `build.cmd`, `run.cmd` 실행 스크립트 추가.
- ViewModel validation 회귀 테스트 4건 추가.

### Changed
- `MainWindow`를 다크 카드형 레이아웃으로 재디자인.
- `SettingsWindow`를 커스텀 헤더 기반 다크 설정 패널 UI로 재디자인.
- `StickyNoteWindow`를 노란 상단 바 + 다크 본문 + 하단 툴바 스타일로 재디자인.
- 테스트 기준을 최신화(총 20개: 통과 18, 건너뜀 2).

### Fixed
- `StickyNoteWindow.xaml`에서 `Button`/`Content` 중복 설정으로 인한 XAML 컴파일 오류 수정.
- 입력 오류가 조용히 기본값으로 대체되던 문제(알람 시간/색상/크기) 개선.

## Notes
- Log updates here before creating a release.
