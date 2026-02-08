namespace DesktopMemo.App.Services;

public interface IUserDialogService
{
    string Prompt(string title, string message, string initialValue = "");
    bool Confirm(string title, string message);
    void Error(string title, string message);
    void Info(string title, string message);
}
