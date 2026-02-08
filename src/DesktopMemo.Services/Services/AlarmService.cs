using System;
using System.Linq;
using CommunityToolkit.WinUI.Notifications;
using DesktopMemo.Data.Infrastructure;
using DesktopMemo.Domain.Enums;
using DesktopMemo.Domain.Interfaces;
using DesktopMemo.Services.Infrastructure;

namespace DesktopMemo.Services.Services;

public class AlarmService : IAlarmService
{
    private readonly IDbContextFactory _contextFactory;
    private readonly IStickyNoteService _stickyNoteService;

    public AlarmService(IDbContextFactory contextFactory, IStickyNoteService stickyNoteService)
    {
        _contextFactory = contextFactory;
        _stickyNoteService = stickyNoteService;
    }

    public void ScheduleOrUpdate(Guid noteId)
    {
        using (var context = _contextFactory.Create())
        {
            var note = context.Notes.FirstOrDefault(x => x.Id == noteId && !x.IsDeleted);
            if (note == null)
            {
                return;
            }

            if (!note.AlarmEnabled)
            {
                note.SnoozeUntilUtc = null;
            }

            if (note.RepeatEndUtc.HasValue && note.AlarmAtUtc.HasValue && note.AlarmAtUtc > note.RepeatEndUtc)
            {
                note.AlarmEnabled = false;
            }

            note.UpdatedAtUtc = DateTime.UtcNow;
            context.SaveChanges();
        }
    }

    public void Dismiss(Guid noteId)
    {
        using (var context = _contextFactory.Create())
        {
            var note = context.Notes.FirstOrDefault(x => x.Id == noteId && !x.IsDeleted);
            if (note == null)
            {
                return;
            }

            note.SnoozeUntilUtc = null;
            if (note.RepeatType == RepeatType.None)
            {
                note.AlarmEnabled = false;
            }
            else
            {
                var baseline = DateTime.UtcNow;
                var next = AlarmCalculator.CalculateNextOccurrence(baseline, note.RepeatType);
                if (!next.HasValue || (note.RepeatEndUtc.HasValue && next.Value > note.RepeatEndUtc.Value))
                {
                    note.AlarmEnabled = false;
                }
                else
                {
                    note.AlarmAtUtc = next;
                    note.AlarmEnabled = true;
                }
            }

            note.UpdatedAtUtc = DateTime.UtcNow;
            context.SaveChanges();
        }
    }

    public void Snooze(Guid noteId, int minutes)
    {
        if (minutes != 5 && minutes != 10 && minutes != 30)
        {
            throw new ArgumentOutOfRangeException(nameof(minutes), "스누즈는 5/10/30분만 지원합니다.");
        }

        using (var context = _contextFactory.Create())
        {
            var note = context.Notes.FirstOrDefault(x => x.Id == noteId && !x.IsDeleted);
            if (note == null)
            {
                return;
            }

            note.SnoozeUntilUtc = DateTime.UtcNow.AddMinutes(minutes);
            note.UpdatedAtUtc = DateTime.UtcNow;
            context.SaveChanges();
        }
    }

    public void ProcessDueAlarms()
    {
        using (var context = _contextFactory.Create())
        {
            var now = DateTime.UtcNow;
            var candidates = context.Notes
                .Where(x => x.AlarmEnabled && !x.IsDeleted)
                .ToList();

            var dueNotes = candidates.Where(x =>
            {
                var trigger = AlarmCalculator.GetEffectiveTriggerUtc(x.AlarmAtUtc, x.SnoozeUntilUtc);
                return trigger.HasValue && trigger.Value <= now;
            }).ToList();

            if (dueNotes.Count == 0)
            {
                return;
            }

            foreach (var note in dueNotes)
            {
                Notify(note.Title);
                _stickyNoteService.OpenSticky(note.Id);

                note.LastTriggeredAtUtc = now;
                note.SnoozeUntilUtc = null;

                if (note.RepeatType == RepeatType.None)
                {
                    note.AlarmEnabled = false;
                    continue;
                }

                var next = AlarmCalculator.CalculateNextOccurrence(now, note.RepeatType);
                if (!next.HasValue || (note.RepeatEndUtc.HasValue && next.Value > note.RepeatEndUtc.Value))
                {
                    note.AlarmEnabled = false;
                }
                else
                {
                    note.AlarmAtUtc = next.Value;
                    note.AlarmEnabled = true;
                }
            }

            context.SaveChanges();
        }
    }

    private static void Notify(string title)
    {
        try
        {
            new ToastContentBuilder()
                .AddText("DesktopMemo")
                .AddText(string.IsNullOrWhiteSpace(title) ? "알람 메모" : title)
                .Show();
        }
        catch
        {
            // Ignore toast failures in unpackaged environments.
        }
    }
}
