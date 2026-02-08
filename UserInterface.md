# DesktopMemo UI Review

## Review Scope
- 대상: `MainWindow`, `SettingsWindow`, `StickyNoteWindow`, `TextPromptWindow`
- 기준: 시각 일관성, 정보 구조, 상호작용 완성도, 접근성, 입력 신뢰성
- 기준 코드 시점: 2026-02-08

## Current State Summary
- 강점:
- 메인 화면 다크 카드형 레이아웃 정착
- 설정창/스티키창 커스텀 다크 UI 적용
- 입력 validation + 오류 피드백 UI 도입
- 남은 핵심 과제:
- 반응형 레이아웃
- 접근성 메타데이터
- Sticky 하단 툴바 기능 연결
- 디자인 토큰 공통화

## Findings (Severity Ordered)

### 1) [High] 메인 화면 고정폭 3단 구조의 반응형 한계
- Reference: `src/DesktopMemo.App/MainWindow.xaml:10`, `src/DesktopMemo.App/MainWindow.xaml:11`, `src/DesktopMemo.App/MainWindow.xaml:267`, `src/DesktopMemo.App/MainWindow.xaml:268`
- Observation:
- `MinWidth=1100` + 고정폭 컬럼(`280`, `360`)으로 작은 화면/DPI 환경에서 정보 밀집이 급격히 증가합니다.
- Recommendation:
- 폭 임계값 기준으로 3단 -> 2단/1단 전환되는 adaptive 레이아웃을 도입하세요.

### 2) [Medium] Sticky 툴바는 시각 요소만 있고 기능이 연결되지 않음
- Reference: `src/DesktopMemo.App/Windows/StickyNoteWindow.xaml:165`, `src/DesktopMemo.App/Windows/StickyNoteWindow.xaml.cs:49`
- Observation:
- 하단 `B/I/U/ab/=/[]` 버튼은 클릭 핸들러가 비어 있습니다.
- Impact:
- 사용자 기대 대비 동작 불일치로 신뢰 저하.
- Recommendation:
- 최소한 `Bold/Italic/Underline`부터 markdown 변환 또는 selection 스타일 동작을 연결하세요.

### 3) [Medium] 접근성 메타데이터 부족
- Reference: `src/DesktopMemo.App/MainWindow.xaml`, `src/DesktopMemo.App/Windows/SettingsWindow.xaml`, `src/DesktopMemo.App/Windows/StickyNoteWindow.xaml`
- Observation:
- `AutomationProperties.Name`, `AccessText`, 키보드 단축키 선언이 거의 없습니다.
- Recommendation:
- 주요 액션(저장/닫기/새 메모/삭제)에 접근성 이름과 액세스 키를 우선 부여하세요.

### 4) [Medium] 디자인 시스템 공통화 부족
- Reference: `src/DesktopMemo.App/MainWindow.xaml`, `src/DesktopMemo.App/Windows/SettingsWindow.xaml`, `src/DesktopMemo.App/Windows/StickyNoteWindow.xaml`
- Observation:
- 창별 색상/버튼 스타일이 유사하지만 독립 정의되어 있습니다.
- Impact:
- 유지보수 비용 증가, 미세한 톤 불일치 발생.
- Recommendation:
- 공통 `ResourceDictionary`로 색상/타이포/버튼 스타일을 통합하세요.

### 5) [Low] 텍스트/숫자 입력 컴포넌트 개선 여지
- Reference: `src/DesktopMemo.App/MainWindow.xaml:408`, `src/DesktopMemo.App/MainWindow.xaml:445`, `src/DesktopMemo.App/Windows/SettingsWindow.xaml:276`
- Observation:
- 유효성은 개선됐지만 `NumericUpDown`, 색상 피커 등 목적형 입력 컨트롤이 아직 없습니다.
- Recommendation:
- 오류 빈도가 높은 필드부터 목적형 컨트롤로 치환하세요.

## Resolved Since Previous UI Review
- `FontSize`, `FontColorHex`, `AlarmTimeText` validation + 오류 피드백 도입 완료.
- `SettingsWindow` 리디자인 완료(커스텀 헤더, 다크 섹션 구조, 저장 액션 강조).
- `StickyNoteWindow` 리디자인 완료(노란 헤더, 다크 본문, 하단 툴바).
- 설정창 리사이즈 가능 정책으로 변경됨.

## Recommended Next Steps
1. 메인 레이아웃 반응형 전환(우선순위 최고)
2. Sticky 툴바 기능 연결(사용자 체감 가치 큼)
3. 공통 UI 리소스 딕셔너리 도입
4. 접근성/키보드 내비게이션 개선
