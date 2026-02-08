using System;
using DesktopMemo.Domain.Enums;

namespace DesktopMemo.Services.Infrastructure;

internal static class AlarmCalculator
{
    public static DateTime? GetEffectiveTriggerUtc(DateTime? alarmAtUtc, DateTime? snoozeUntilUtc)
    {
        if (snoozeUntilUtc.HasValue)
        {
            return snoozeUntilUtc;
        }

        return alarmAtUtc;
    }

    public static DateTime? CalculateNextOccurrence(DateTime currentUtc, RepeatType repeatType)
    {
        switch (repeatType)
        {
            case RepeatType.Daily:
                return currentUtc.AddDays(1);
            case RepeatType.Weekly:
                return currentUtc.AddDays(7);
            case RepeatType.Monthly:
                return currentUtc.AddMonths(1);
            default:
                return null;
        }
    }
}
