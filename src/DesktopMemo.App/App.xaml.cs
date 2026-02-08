using System;
using System.Threading;
using System.Windows;
using DesktopMemo.App.Services;
using DesktopMemo.App.ViewModels;
using DesktopMemo.Data.Persistence;
using DesktopMemo.Domain.Interfaces;
using DesktopMemo.Services;
using DesktopMemo.Services.Services;
using Microsoft.Extensions.DependencyInjection;

namespace DesktopMemo.App;

public partial class App : Application
{
    private Mutex _singleInstanceMutex;
    private ServiceProvider _serviceProvider;
    private TrayIconService _trayIconService;
    private AlarmBackgroundWorker _alarmBackgroundWorker;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        bool createdNew;
        _singleInstanceMutex = new Mutex(true, "DesktopMemo.SingleInstance", out createdNew);
        if (!createdNew)
        {
            MessageBox.Show("DesktopMemo는 이미 실행 중입니다.", "안내", MessageBoxButton.OK, MessageBoxImage.Information);
            Shutdown();
            return;
        }

        DatabaseBootstrapper.Initialize();

        var services = new ServiceCollection();
        ConfigureServices(services);
        _serviceProvider = services.BuildServiceProvider();

        _trayIconService = _serviceProvider.GetRequiredService<TrayIconService>();
        _alarmBackgroundWorker = _serviceProvider.GetRequiredService<AlarmBackgroundWorker>();
        _alarmBackgroundWorker.Start();

        MainWindow = _serviceProvider.GetRequiredService<MainWindow>();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _alarmBackgroundWorker?.Dispose();
        _trayIconService?.Dispose();
        _serviceProvider?.Dispose();

        if (_singleInstanceMutex != null)
        {
            _singleInstanceMutex.ReleaseMutex();
            _singleInstanceMutex.Dispose();
        }

        base.OnExit(e);
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        services.AddDesktopMemoCore();

        services.AddSingleton<AppState>();
        services.AddSingleton<IUserDialogService, UserDialogService>();

        services.AddSingleton<IStickyNoteService, StickyNoteWindowService>();
        services.AddSingleton<IAlarmService, AlarmService>();
        services.AddSingleton<IAppLifecycleService, AppLifecycleService>();

        services.AddSingleton<MainViewModel>();
        services.AddSingleton<MainWindow>();

        services.AddTransient<SettingsViewModel>();
        services.AddTransient<Windows.SettingsWindow>();

        services.AddSingleton<TrayIconService>();
        services.AddSingleton<AlarmBackgroundWorker>();
    }
}
