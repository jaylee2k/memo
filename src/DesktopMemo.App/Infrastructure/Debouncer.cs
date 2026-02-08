using System;
using System.Windows.Threading;

namespace DesktopMemo.App.Infrastructure;

public sealed class Debouncer : IDisposable
{
    private readonly DispatcherTimer _timer;
    private Action _action;

    public Debouncer(TimeSpan delay)
    {
        _timer = new DispatcherTimer
        {
            Interval = delay
        };

        _timer.Tick += OnTick;
    }

    public void Bounce(Action action)
    {
        _action = action;
        _timer.Stop();
        _timer.Start();
    }

    private void OnTick(object sender, EventArgs e)
    {
        _timer.Stop();
        var action = _action;
        _action = null;
        action?.Invoke();
    }

    public void Dispose()
    {
        _timer.Stop();
        _timer.Tick -= OnTick;
    }
}
