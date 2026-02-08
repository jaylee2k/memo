using System.Windows;

namespace DesktopMemo.App.Windows;

public partial class TextPromptWindow : Window
{
    public string ResultText { get; private set; }

    public TextPromptWindow(string title, string message, string initialValue)
    {
        InitializeComponent();

        Title = title;
        MessageText.Text = message;
        InputText.Text = initialValue ?? string.Empty;
        InputText.SelectAll();
        InputText.Focus();

        ResultText = string.Empty;
    }

    private void OkButton_OnClick(object sender, RoutedEventArgs e)
    {
        ResultText = InputText.Text?.Trim();
        DialogResult = true;
    }

    private void CancelButton_OnClick(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
    }
}
