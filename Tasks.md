# Tasks

## Backlog
- [ ] 반복 알람 계산 로직 개선(기준 시각 유지 + DST 고려).
- [ ] `TrashService.DeleteItemPermanently` 예외 UI 처리 보강(`MainViewModel` try/catch + 사용자 메시지).
- [ ] `App` 단일 인스턴스 뮤텍스 해제 안정화(`ReleaseMutex` 소유권 체크).
- [ ] Sticky Note 하단 툴바 버튼 기능 연결(B/I/U/취소선/목록/이미지).
- [ ] 메인 레이아웃 반응형 전환(고정폭 3단 -> 폭 기준 2단/1단).
- [ ] 설치 파일 코드서명 절차 추가.

## In Progress
- [ ] 없음.

## Done
- [x] Repository docs initialized.
- [x] 솔루션 구조(App/Domain/Data/Services) 생성 및 참조 연결.
- [x] WPF 메인 화면 3단 레이아웃 + 휴지통 탭 구현.
- [x] 그룹/메모/휴지통/설정 서비스 구현.
- [x] Sticky Note 다중 창 및 창 상태 저장 구현.
- [x] 알람/스누즈/반복 및 토스트 알림 처리 구현.
- [x] 트레이 아이콘 동작(클릭/더블클릭/종료 메뉴) 구현.
- [x] Markdown 미리보기 기능 구현.
- [x] 휴지통 선택 영구삭제 기능 구현.
- [x] SQLite 마이그레이션/FOREIGN KEY 관련 런타임 오류 수정.
- [x] 메모 제목 검색/필터 기능 구현.
- [x] 메인/설정 입력값 validation 및 오류 피드백 UI 구현.
- [x] `MainWindow`/`SettingsWindow`/`StickyNoteWindow` 다크 UI 리디자인.
- [x] MSTest V2 테스트 20개 구성(통과 18, 건너뜀 2).
- [x] 패키징 스크립트(Inno Setup) 문서화.

## Notes
- Move items between sections as status changes.
