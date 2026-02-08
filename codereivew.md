# DesktopMemo Code Review

## Review Summary
- Review scope: `src/` + `tests/` 전체 코드
- Test baseline: `dotnet test tests/DesktopMemo.Tests/DesktopMemo.Tests.csproj -c Debug` 통과 (`Passed 14`, `Skipped 2`, `Failed 0`)
- 결론: 즉시 수정이 필요한 결함 2건(High), 구조적/안정성 리스크 4건(Medium/Low)

## Findings (Severity Ordered)

### 1) [High] 반복 알람 시간이 점점 드리프트됨 (원래 설정 시각 유지 실패)
- Location: `src/DesktopMemo.Services/Services/AlarmService.cs:64`, `src/DesktopMemo.Services/Services/AlarmService.cs:137`, `src/DesktopMemo.Services/Infrastructure/AlarmCalculator.cs:18`
- Problem: 다음 알람 계산 기준을 "원래 알람 시각"이 아니라 `DateTime.UtcNow`로 사용합니다.
- Impact: 사용자가 늦게 `Dismiss`할수록 반복 알람 시간이 밀립니다. DST 전환 시에도 로컬 고정 시각을 유지하지 못합니다.
- Recommendation: 다음 발생 시각 계산 시 `note.AlarmAtUtc`(또는 로컬 기준 원본 시각 + TimeZoneId)을 기준으로 산출하고, DST/타임존 보정 로직을 포함한 recurrence 계산기로 분리하세요.

### 2) [High] 휴지통 영구삭제에서 예외가 UI 계층으로 전파되어 앱이 중단될 수 있음
- Location: `src/DesktopMemo.App/ViewModels/MainViewModel.cs:858`, `src/DesktopMemo.Services/Services/TrashService.cs:146`
- Problem: `TrashService.DeleteItemPermanently`는 활성 하위 항목이 있으면 `InvalidOperationException`을 던지지만, `MainViewModel.DeleteTrashItemPermanently`에서 try/catch가 없습니다.
- Impact: 사용자 동작(영구삭제)만으로 UI 스레드 예외가 전파되어 앱 크래시 가능성이 있습니다.
- Recommendation: `DeleteTrashItemPermanently`를 try/catch로 감싸 사용자 메시지 처리(`IUserDialogService.Error`) 후 상태 메시지를 업데이트하세요.

### 3) [Medium] 세컨드 인스턴스 종료 경로에서 `ReleaseMutex()` 예외 가능
- Location: `src/DesktopMemo.App/App.xaml.cs:26`, `src/DesktopMemo.App/App.xaml.cs:27`, `src/DesktopMemo.App/App.xaml.cs:55`
- Problem: 기존 인스턴스가 있을 때(`createdNew == false`)도 `OnExit`에서 무조건 `_singleInstanceMutex.ReleaseMutex()`를 호출합니다.
- Impact: 해당 프로세스가 뮤텍스 소유권이 없으면 `ApplicationException`이 발생할 수 있습니다.
- Recommendation: 소유권 획득 여부 플래그를 별도로 저장해 소유한 경우에만 `ReleaseMutex()` 호출하세요.

### 4) [Medium] Legacy GUID 정규화가 트랜잭션 없이 파괴적 재작성 수행
- Location: `src/DesktopMemo.Data/Persistence/DatabaseBootstrapper.cs:101`, `src/DesktopMemo.Data/Persistence/DatabaseBootstrapper.cs:102`, `src/DesktopMemo.Data/Persistence/DatabaseBootstrapper.cs:163`
- Problem: `NormalizeLegacyGuidStorage`가 FK를 끄고 전체 삭제 후 재삽입을 수행하지만 트랜잭션/롤백 보호가 없습니다.
- Impact: 중간 실패 시 데이터 유실/부분 복구 상태가 남을 수 있습니다.
- Recommendation: 단일 DB 트랜잭션으로 감싸고, 예외 시 롤백 + FK 재활성화를 `finally`에서 보장하세요.

### 5) [Medium] 백그라운드 작업이 UI DispatcherTimer에서 실행되어 프리징 리스크
- Location: `src/DesktopMemo.App/Services/AlarmBackgroundWorker.cs:11`, `src/DesktopMemo.App/Services/AlarmBackgroundWorker.cs:43`, `src/DesktopMemo.App/Services/AlarmBackgroundWorker.cs:48`
- Problem: 알람 처리/휴지통 정리가 UI 스레드 타이머에서 동작합니다.
- Impact: DB I/O 또는 토스트/윈도우 오픈 지연 시 UI 응답성 저하가 발생할 수 있습니다.
- Recommendation: `System.Threading.Timer` 또는 백그라운드 Task 기반으로 이전하고, UI 접근이 필요한 부분만 Dispatcher로 마샬링하세요.

### 6) [Low] 트레이 아이콘 더블클릭 시 단일클릭 핸들러가 함께 발화될 가능성
- Location: `src/DesktopMemo.App/Services/TrayIconService.cs:24`, `src/DesktopMemo.App/Services/TrayIconService.cs:25`, `src/DesktopMemo.App/Services/TrayIconService.cs:39`, `src/DesktopMemo.App/Services/TrayIconService.cs:44`
- Problem: `Click`과 `DoubleClick`에 서로 다른 동작을 동시에 바인딩했습니다.
- Impact: 플랫폼/타이밍에 따라 더블클릭 시 메인창 + 설정창이 함께 열리는 비일관 동작이 발생할 수 있습니다.
- Recommendation: `MouseClick`에서 버튼/클릭횟수를 분기하거나, 단일 인터랙션 정책으로 단순화하세요.

## Testing Gaps
- `TrashService.DeleteItemPermanently` 예외 케이스(활성 하위 항목 존재)와 `MainViewModel` 예외 처리 경로 테스트가 없습니다.
- 반복 알람의 "원 시각 유지", DST 경계, 타임존 전환 케이스 테스트가 없습니다.
- 뮤텍스 중복 실행 경로(세컨드 인스턴스 시작/종료) 회귀 테스트가 없습니다.

## Suggested Fix Priority
1. Finding 1, 2 (알람 정합성 + 크래시 방지)
2. Finding 3, 4 (프로세스 안정성 + 데이터 안전성)
3. Finding 5, 6 (UX/운영 품질 개선)

