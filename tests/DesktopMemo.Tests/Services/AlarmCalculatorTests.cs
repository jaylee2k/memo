using System;
using DesktopMemo.Domain.Enums;
using DesktopMemo.Services.Infrastructure;
using DesktopMemo.Services.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DesktopMemo.Tests.Services;

[TestClass]
public class AlarmCalculatorTests
{
    [TestMethod]
    public void GetEffectiveTriggerUtc_ReturnsSnooze_WhenSnoozeExists()
    {
        var alarm = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var snooze = alarm.AddMinutes(10);

        var result = AlarmCalculator.GetEffectiveTriggerUtc(alarm, snooze);

        Assert.AreEqual(snooze, result);
    }

    [TestMethod]
    public void GetEffectiveTriggerUtc_ReturnsAlarm_WhenSnoozeMissing()
    {
        var alarm = new DateTime(2026, 1, 1, 8, 0, 0, DateTimeKind.Utc);

        var result = AlarmCalculator.GetEffectiveTriggerUtc(alarm, null);

        Assert.AreEqual(alarm, result);
    }

    [DataTestMethod]
    [DataRow(RepeatType.Daily, 1)]
    [DataRow(RepeatType.Weekly, 7)]
    [DataRow(RepeatType.Monthly, 31)]
    public void CalculateNextOccurrence_ReturnsExpectedDate(RepeatType repeatType, int expectedDayDelta)
    {
        var current = new DateTime(2026, 1, 1, 9, 0, 0, DateTimeKind.Utc);

        var result = AlarmCalculator.CalculateNextOccurrence(current, repeatType);

        Assert.IsNotNull(result);
        Assert.AreEqual(current.AddDays(expectedDayDelta), result.Value);
    }

    [TestMethod]
    public void CalculateNextOccurrence_ReturnsNull_ForNone()
    {
        var current = new DateTime(2026, 1, 1, 9, 0, 0, DateTimeKind.Utc);

        var result = AlarmCalculator.CalculateNextOccurrence(current, RepeatType.None);

        Assert.IsNull(result);
    }

    [TestMethod]
    public void Snooze_Throws_ForUnsupportedMinutes()
    {
        var service = new AlarmService(null, null);

        Assert.ThrowsException<ArgumentOutOfRangeException>(() => service.Snooze(Guid.NewGuid(), 7));
    }
}
