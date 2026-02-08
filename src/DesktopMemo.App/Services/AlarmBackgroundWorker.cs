using System;
using System.Windows.Threading;
using DesktopMemo.Domain.Interfaces;

namespace DesktopMemo.App.Services;

public sealed class AlarmBackgroundWorker : IDisposable
{
    private readonly IAlarmService _alarmService;
    private readonly ITrashService _trashService;
    private readonly DispatcherTimer _alarmTimer;
    private readonly DispatcherTimer _purgeTimer;

    public AlarmBackgroundWorker(IAlarmService alarmService, ITrashService trashService)
    {
        _alarmService = alarmService;
        _trashService = trashService;

        _alarmTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(30)
        };
        _alarmTimer.Tick += OnAlarmTick;

        _purgeTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromHours(6)
        };
        _purgeTimer.Tick += OnPurgeTick;
    }

    public void Start()
    {
        _alarmTimer.Start();
        _purgeTimer.Start();

        _alarmService.ProcessDueAlarms();
        _trashService.PurgeExpiredItems();
    }

    private void OnAlarmTick(object sender, EventArgs e)
    {
        _alarmService.ProcessDueAlarms();
    }

    private void OnPurgeTick(object sender, EventArgs e)
    {
        _trashService.PurgeExpiredItems();
    }

    public void Dispose()
    {
        _alarmTimer.Stop();
        _alarmTimer.Tick -= OnAlarmTick;

        _purgeTimer.Stop();
        _purgeTimer.Tick -= OnPurgeTick;
    }
}
