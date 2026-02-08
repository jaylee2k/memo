using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using DesktopMemo.App.Services;
using DesktopMemo.App.ViewModels;

namespace DesktopMemo.App;

public partial class MainWindow : Window
{
    private readonly MainViewModel _viewModel;
    private readonly AppState _appState;

    public MainWindow(MainViewModel viewModel, AppState appState)
    {
        _viewModel = viewModel;
        _appState = appState;

        InitializeComponent();
        DataContext = _viewModel;

        _viewModel.PropertyChanged += ViewModelOnPropertyChanged;
        Loaded += (_, _) =>
        {
            _viewModel.Initialize();
            RenderMarkdownPreview();
        };
    }

    private void GroupTree_OnSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
    {
        _viewModel.SelectedGroup = e.NewValue as GroupNodeViewModel;
    }

    protected override void OnClosing(CancelEventArgs e)
    {
        if (!_appState.IsExiting)
        {
            e.Cancel = true;
            Hide();
            return;
        }

        _viewModel.PropertyChanged -= ViewModelOnPropertyChanged;
        base.OnClosing(e);
    }

    private void ViewModelOnPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(MainViewModel.MarkdownPreviewHtml))
        {
            return;
        }

        RenderMarkdownPreview();
    }

    private void RenderMarkdownPreview()
    {
        if (MarkdownPreviewBrowser == null)
        {
            return;
        }

        MarkdownPreviewBrowser.NavigateToString(_viewModel.MarkdownPreviewHtml ?? "<html><body></body></html>");
    }
}
