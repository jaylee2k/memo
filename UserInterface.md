# DesktopMemo UI Review

## Review Scope
- 대상: `MainWindow`, `SettingsWindow`, `StickyNoteWindow`, `TextPromptWindow` + UI 연관 ViewModel 로직
- 관점: 시각적 일관성, 정보 구조, 상호작용 안정성, 접근성(Accessibility), 입력 신뢰성

## Executive Summary
- 현재 UI는 메인 화면의 다크 테마와 카드 구조는 방향성이 좋습니다.
- 다만 창 간 디자인 시스템 불일치, 입력 검증 부재, 접근성 메타데이터 누락이 명확합니다.
- 특히 알람 시간 입력 처리와 반응형 레이아웃 제약은 사용성 리스크가 큽니다.

## Findings (Severity Ordered)

### 1) [High] 레이아웃이 고정폭 중심이라 화면 크기 변화 대응이 약함
- Reference: `src/DesktopMemo.App/MainWindow.xaml:10`, `src/DesktopMemo.App/MainWindow.xaml:11`, `src/DesktopMemo.App/MainWindow.xaml:260`, `src/DesktopMemo.App/MainWindow.xaml:261`
- Observation:
- `MinWidth=1100`, `MinHeight=680`로 작은 화면 대응이 제한됨.
- 핵심 3단 레이아웃이 `280 + 360 + *` 고정 폭 조합이라 정보 밀도 변화에 취약.
- Impact:
- 125%/150% DPI, 노트북 해상도, 다중 모니터 축소 배치에서 가독성/조작성이 급격히 저하될 수 있음.
- Recommendation:
- 폭 기준 `GridLength`를 `*` 비율 중심으로 재설계하고, 임계 폭 이하에서 2단/1단으로 전환하는 adaptive 레이아웃을 도입.

### 2) [High] 입력 검증과 오류 피드백이 UI에 노출되지 않음
- Reference: `src/DesktopMemo.App/MainWindow.xaml:394`, `src/DesktopMemo.App/MainWindow.xaml:398`, `src/DesktopMemo.App/MainWindow.xaml:431`, `src/DesktopMemo.App/ViewModels/MainViewModel.cs:603`, `src/DesktopMemo.App/ViewModels/MainViewModel.cs:610`, `src/DesktopMemo.App/Windows/SettingsWindow.xaml:20`, `src/DesktopMemo.App/Windows/SettingsWindow.xaml:29`
- Observation:
- `FontSize`, `FontColorHex`, `AlarmTimeText`가 자유 텍스트 입력.
- `AlarmTimeText` 파싱 실패 시 경고 없이 09:00으로 대체됨.
- Impact:
- 사용자가 잘못 입력해도 즉시 인지하지 못해 예상과 다른 저장/알람 동작 발생.
- Recommendation:
- `ValidationRule` 또는 `IDataErrorInfo`로 실시간 검증 추가.
- 알람 시간 포맷 오류는 명시적 에러 메시지 + 저장 차단(또는 사용자 확인)으로 변경.

### 3) [High] 창 간 디자인 시스템이 일관되지 않음
- Reference: `src/DesktopMemo.App/MainWindow.xaml:220`, `src/DesktopMemo.App/Windows/SettingsWindow.xaml:7`, `src/DesktopMemo.App/Windows/StickyNoteWindow.xaml:4`, `src/DesktopMemo.App/Windows/TextPromptWindow.xaml:4`
- Observation:
- 메인창은 다크 테마/커스텀 스타일, 반면 설정/스티키/프롬프트는 기본 WPF 룩.
- 타이틀 언어도 혼합(`메모 기록` vs `Sticky Note`).
- Impact:
- 제품 인상이 분절되고, 같은 앱 내부에서도 맥락 전환 비용이 큼.
- Recommendation:
- 공통 `ResourceDictionary`(색상/타이포/컨트롤 스타일)로 통합.
- 창 제목/버튼 문구 로컬라이제이션 정책을 통일.

### 4) [Medium] 접근성(키보드/스크린리더) 메타데이터가 사실상 없음
- Reference: `src/DesktopMemo.App/MainWindow.xaml`, `src/DesktopMemo.App/Windows/SettingsWindow.xaml`, `src/DesktopMemo.App/Windows/StickyNoteWindow.xaml`, `src/DesktopMemo.App/Windows/TextPromptWindow.xaml` (검색 결과 `AutomationProperties`, `AccessText`, `KeyBinding`, `InputBinding` 없음)
- Observation:
- 컨트롤 접근성 이름, 키보드 단축키, 액세스 키 체계가 정의되지 않음.
- Impact:
- 키보드 중심 사용자와 보조기술 사용자 경험이 크게 저하됨.
- Recommendation:
- 주요 버튼/입력 필드에 `AutomationProperties.Name` 지정.
- 빈도가 높은 액션(`새 메모`, `삭제`, `저장`)에 키바인딩 및 액세스 키 도입.

### 5) [Medium] 상단/하단 액션이 많아 우선순위 인지가 어려움
- Reference: `src/DesktopMemo.App/MainWindow.xaml:220`, `src/DesktopMemo.App/MainWindow.xaml:234`, `src/DesktopMemo.App/MainWindow.xaml:448`
- Observation:
- 상단 액션 버튼 밀도가 높고, 하단에도 강한 액션이 병렬 배치되어 있음.
- 1차(primary)와 2차(secondary) 액션의 시각적 계층이 약함.
- Impact:
- 신규 사용자 기준 “다음에 무엇을 해야 하는지” 파악이 느림.
- Recommendation:
- CTA 1개를 명확히 강조(예: `새 메모`), 나머지는 분리/드롭다운/컨텍스트 메뉴로 정리.
- 위험 액션(삭제류)은 시각/위치적으로 분리 유지.

### 6) [Medium] 검색 입력의 발견성(Discoverability)이 낮음
- Reference: `src/DesktopMemo.App/MainWindow.xaml:249`
- Observation:
- 검색 안내가 `ToolTip`에만 있어 기본 상태에서 용도가 직관적으로 드러나지 않음.
- Recommendation:
- 플레이스홀더 텍스트(또는 라벨)와 검색 아이콘을 입력창 내부/외부에 상시 노출.

### 7) [Low] 다크 테마 내 미리보기 영역이 순백 배경으로 시각적 단절 발생
- Reference: `src/DesktopMemo.App/MainWindow.xaml:378`
- Observation:
- Markdown `WebBrowser` 컨테이너가 `#FFFFFF`로 고정되어 주변 다크 톤과 강한 대비를 만듦.
- Recommendation:
- 미리보기 테마를 다크/라이트 선택 가능하게 하거나 최소 중립 톤으로 완화.

### 8) [Low] 모달 창들의 크기/리사이즈 정책이 사용자 제어를 제한
- Reference: `src/DesktopMemo.App/Windows/SettingsWindow.xaml:8`, `src/DesktopMemo.App/Windows/TextPromptWindow.xaml:8`
- Observation:
- 설정/프롬프트 창이 `NoResize`.
- Recommendation:
- 최소/최대 크기 범위를 두고 리사이즈 허용, 또는 컨텐츠 양이 늘어도 스크롤/랩 정책을 명확히.

## Positive Notes
- 메인 화면의 다크 테마, 카드형 리스트, 상태 메시지 분리 구조는 현대적이며 확장 가능성이 높음.
- 휴지통 탭의 기능 분리(복구/영구삭제/정리)는 사용자 작업 모델과 잘 맞음.
- 스누즈/알람 관련 액션을 화면 하단에 분리한 점은 작업 구분 측면에서 적절함.

## Recommended Improvement Plan
1. 입력 검증(시간/숫자/HEX)과 사용자 피드백 체계부터 도입.
2. 공통 UI 리소스 딕셔너리로 창 전역 스타일/로컬라이제이션 통합.
3. 메인 화면 레이아웃을 adaptive 구조로 전환(폭 기준 재배치).
4. 접근성 메타데이터 + 키보드 단축키를 1차 시나리오부터 적용.
5. 액션 우선순위 재설계(Primary/Secondary/Danger 계층 명확화).

