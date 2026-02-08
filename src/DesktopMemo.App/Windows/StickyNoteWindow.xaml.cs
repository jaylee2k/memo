using System;
using System.Windows;
using System.Windows.Input;
using DesktopMemo.App.ViewModels;
using DesktopMemo.Domain.Contracts;
using DesktopMemo.Domain.Interfaces;

namespace DesktopMemo.App.Windows;

public partial class StickyNoteWindow : Window
{
    private readonly StickyNoteViewModel _viewModel;
    private readonly IStickyNoteService _stickyNoteService;

    public StickyNoteWindow(StickyNoteViewModel viewModel, IStickyNoteService stickyNoteService)
    {
        _viewModel = viewModel;
        _stickyNoteService = stickyNoteService;

        InitializeComponent();
        DataContext = _viewModel;
    }

    private void TopBar_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.LeftButton != MouseButtonState.Pressed)
        {
            return;
        }

        try
        {
            DragMove();
        }
        catch
        {
            // Ignore drag exceptions when mouse state changes unexpectedly.
        }
    }

    private void AddButton_OnClick(object sender, RoutedEventArgs e)
    {
    }

    private void MoreButton_OnClick(object sender, RoutedEventArgs e)
    {
    }

    private void ToolbarButton_OnClick(object sender, RoutedEventArgs e)
    {
    }

    private void CloseButton_OnClick(object sender, RoutedEventArgs e)
    {
        Close();
    }

    protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
    {
        _viewModel.Save();

        _stickyNoteService.SaveWindowState(_viewModel.NoteId, new StickyWindowStateDto
        {
            Left = Left,
            Top = Top,
            Width = Width,
            Height = Height,
            IsAlwaysOnTop = Topmost,
            LastOpenedAtUtc = DateTime.UtcNow
        });

        base.OnClosing(e);
    }
}
