using System;

namespace DesktopMemo.Domain.Interfaces;

public interface IAlarmService
{
    void ScheduleOrUpdate(Guid noteId);
    void Dismiss(Guid noteId);
    void Snooze(Guid noteId, int minutes);
    void ProcessDueAlarms();
}
