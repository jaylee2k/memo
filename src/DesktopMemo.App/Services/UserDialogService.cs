using System.Windows;

namespace DesktopMemo.App.Services;

public class UserDialogService : IUserDialogService
{
    public string Prompt(string title, string message, string initialValue = "")
    {
        var prompt = new Windows.TextPromptWindow(title, message, initialValue)
        {
            Owner = Application.Current.MainWindow
        };

        var result = prompt.ShowDialog();
        return result == true ? prompt.ResultText : null;
    }

    public bool Confirm(string title, string message)
    {
        return MessageBox.Show(message, title, MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes;
    }

    public void Error(string title, string message)
    {
        MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
    }

    public void Info(string title, string message)
    {
        MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);
    }
}
