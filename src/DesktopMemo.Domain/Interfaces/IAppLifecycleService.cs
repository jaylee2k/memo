namespace DesktopMemo.Domain.Interfaces;

public interface IAppLifecycleService
{
    void ExitApplication();
    void ShowMainWindow();
    void ShowSettingsWindow();
}
