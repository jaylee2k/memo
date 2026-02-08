using System;
using System.Drawing;
using System.Windows.Forms;
using DesktopMemo.Domain.Interfaces;

namespace DesktopMemo.App.Services;

public sealed class TrayIconService : IDisposable
{
    private readonly NotifyIcon _notifyIcon;
    private readonly IAppLifecycleService _lifecycleService;

    public TrayIconService(IAppLifecycleService lifecycleService)
    {
        _lifecycleService = lifecycleService;
        _notifyIcon = new NotifyIcon
        {
            Text = "DesktopMemo",
            Icon = SystemIcons.Application,
            Visible = true,
            ContextMenuStrip = BuildMenu()
        };

        _notifyIcon.Click += OnClick;
        _notifyIcon.DoubleClick += OnDoubleClick;
    }

    private ContextMenuStrip BuildMenu()
    {
        var menu = new ContextMenuStrip();
        menu.Items.Add("메모 관리함", null, (_, _) => _lifecycleService.ShowMainWindow());
        menu.Items.Add("환경설정", null, (_, _) => _lifecycleService.ShowSettingsWindow());
        menu.Items.Add("종료", null, (_, _) => _lifecycleService.ExitApplication());
        return menu;
    }

    private void OnClick(object sender, EventArgs e)
    {
        _lifecycleService.ShowMainWindow();
    }

    private void OnDoubleClick(object sender, EventArgs e)
    {
        _lifecycleService.ShowSettingsWindow();
    }

    public void Dispose()
    {
        _notifyIcon.Click -= OnClick;
        _notifyIcon.DoubleClick -= OnDoubleClick;
        _notifyIcon.Visible = false;
        _notifyIcon.Dispose();
    }
}
