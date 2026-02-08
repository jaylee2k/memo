using System;
using System.Windows;
using DesktopMemo.Domain.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace DesktopMemo.App.Services;

public class AppLifecycleService : IAppLifecycleService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly AppState _appState;
    private MainWindow _mainWindow;
    private Windows.SettingsWindow _settingsWindow;

    public AppLifecycleService(IServiceProvider serviceProvider, AppState appState)
    {
        _serviceProvider = serviceProvider;
        _appState = appState;
    }

    public MainWindow GetOrCreateMainWindow()
    {
        if (_mainWindow == null)
        {
            _mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
            Application.Current.MainWindow = _mainWindow;
        }

        return _mainWindow;
    }

    public void ShowMainWindow()
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            var window = GetOrCreateMainWindow();
            if (!window.IsVisible)
            {
                window.Show();
            }

            if (window.WindowState == WindowState.Minimized)
            {
                window.WindowState = WindowState.Normal;
            }

            window.Activate();
        });
    }

    public void ShowSettingsWindow()
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            if (_settingsWindow == null || !_settingsWindow.IsLoaded)
            {
                _settingsWindow = _serviceProvider.GetRequiredService<Windows.SettingsWindow>();
                _settingsWindow.Closed += (_, _) => _settingsWindow = null;
                _settingsWindow.Show();
                return;
            }

            if (_settingsWindow.WindowState == WindowState.Minimized)
            {
                _settingsWindow.WindowState = WindowState.Normal;
            }

            _settingsWindow.Activate();
        });
    }

    public void ExitApplication()
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            _appState.IsExiting = true;

            if (_settingsWindow != null)
            {
                _settingsWindow.Close();
            }

            if (_mainWindow != null)
            {
                _mainWindow.Close();
            }

            Application.Current.Shutdown();
        });
    }
}
