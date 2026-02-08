# DesktopMemo Code Review

## Review Summary
- Scope: `src/` + `tests/` 전체 코드 기준 최신 상태 재검토
- Test baseline: `dotnet test tests/DesktopMemo.Tests/DesktopMemo.Tests.csproj -c Release` (`Passed 18`, `Skipped 2`, `Failed 0`)
- 결론: 안정성/정합성 이슈 5건이 남아 있으며, 즉시 보강 권장 항목은 2건입니다.

## Findings (Severity Ordered)

### 1) [High] 반복 알람 기준 시각 드리프트
- Location: `src/DesktopMemo.Services/Services/AlarmService.cs:64`, `src/DesktopMemo.Services/Services/AlarmService.cs:137`, `src/DesktopMemo.Services/Infrastructure/AlarmCalculator.cs:18`
- Problem: 반복 알람의 다음 시각 계산 기준이 `DateTime.UtcNow` 중심입니다.
- Impact: 사용자가 늦게 해제할수록 알람 시각이 밀리고, DST/타임존 경계에서 기대 동작과 어긋날 수 있습니다.
- Recommendation: `AlarmAtUtc` + `TimeZoneId` 기반 기준 시각 유지 방식으로 recurrence 계산기를 개선하세요.

### 2) [High] 휴지통 영구삭제 예외 UI 처리 미흡
- Location: `src/DesktopMemo.Services/Services/TrashService.cs:146`, `src/DesktopMemo.App/ViewModels/MainViewModel.cs:923`
- Problem: 서비스에서 `InvalidOperationException`이 발생 가능한데 UI 명령에서 예외를 처리하지 않습니다.
- Impact: 사용자 영구삭제 동작 중 예외가 UI 스레드로 전파될 수 있습니다.
- Recommendation: `DeleteTrashItemPermanently`를 try/catch로 감싸고 명시적 사용자 메시지를 제공하세요.

### 3) [Medium] Mutex 해제 안전성
- Location: `src/DesktopMemo.App/App.xaml.cs:26`, `src/DesktopMemo.App/App.xaml.cs:55`
- Problem: 세컨드 인스턴스 경로에서도 `ReleaseMutex()`를 호출할 가능성이 있습니다.
- Impact: 소유권 없는 뮤텍스 해제 시 런타임 예외 가능.
- Recommendation: 소유권 플래그를 저장하고 소유한 경우에만 해제하세요.

### 4) [Medium] Legacy GUID 정규화의 트랜잭션 안전성 부족
- Location: `src/DesktopMemo.Data/Persistence/DatabaseBootstrapper.cs:101`, `src/DesktopMemo.Data/Persistence/DatabaseBootstrapper.cs:163`
- Problem: FK OFF + 삭제/재삽입 절차가 트랜잭션 보호 없이 수행됩니다.
- Impact: 중간 실패 시 부분 복구/데이터 손실 리스크.
- Recommendation: 전체 정규화 루틴을 DB 트랜잭션으로 감싸고 `finally`에서 FK ON을 보장하세요.

### 5) [Medium] UI DispatcherTimer에서 DB 작업 수행
- Location: `src/DesktopMemo.App/Services/AlarmBackgroundWorker.cs:11`, `src/DesktopMemo.App/Services/AlarmBackgroundWorker.cs:43`, `src/DesktopMemo.App/Services/AlarmBackgroundWorker.cs:48`
- Problem: 주기 작업이 UI 스레드 타이머에서 실행됩니다.
- Impact: DB I/O 지연 시 UI responsiveness 저하 가능.
- Recommendation: 백그라운드 타이머/Task로 이전하고 UI 접근만 Dispatcher로 분리하세요.

## Resolved Since Previous Review
- 메인/설정 입력 validation 부재 문제는 해결되었습니다.
- validation UI 피드백(오류 보더/툴팁) 및 저장 차단이 반영되었습니다.
- UI 리디자인(`MainWindow`, `SettingsWindow`, `StickyNoteWindow`)이 적용되었습니다.

## Testing Gaps
- `MainViewModel.DeleteTrashItemPermanently` 예외 경로 테스트 부재.
- 반복 알람 기준 시각 유지/DST 시나리오 테스트 부재.
- 단일 인스턴스 뮤텍스 소유권 경로 테스트 부재.

## Recommended Priority
1. Finding 1, 2
2. Finding 3, 4
3. Finding 5
